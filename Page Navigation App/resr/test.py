import argparse
import cv2
import glob
import os
from basicsr.archs.rrdbnet_arch import RRDBNet
from utils import RealESRGANer


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('-i', '--input', type=str, default='inputs', help='Input image or folder')
    parser.add_argument('-n','--model_name',type=str,default='RealESRGAN',)
    parser.add_argument('-o', '--output', type=str, default='results')
    parser.add_argument('-dn','--denoise_strength',type=float,default=0.5,)
    parser.add_argument('-s', '--outscale', type=float, default=4)
    parser.add_argument('--model_path', type=str, default=None)
    parser.add_argument('--suffix', type=str, default='out')
    parser.add_argument('-t', '--tile', type=int, default=0)
    parser.add_argument('--tile_pad', type=int, default=10)
    parser.add_argument('--pre_pad', type=int, default=0)
    parser.add_argument('--face_enhance', action='store_true')
    parser.add_argument('--fp32', action='store_true')
    parser.add_argument('--alpha_upsampler',type=str,default='realesrgan')
    parser.add_argument('--ext',type=str,default='auto')
    parser.add_argument('-g', '--gpu-id', type=int, default=None)
    args = parser.parse_args()
    args.model_name = args.model_name.split('.')[0]
    if args.model_name == 'RealESRGAN':
        model = RRDBNet(num_in_ch=3, num_out_ch=3, num_feat=64, num_block=23, num_grow_ch=32, scale=4)
        netscale = 4
    if args.model_path is not None:
        model_path = args.model_path
    else:
        model_path = os.path.join('weights', args.model_name + '.pth')
    dni_weight = None
    upsampler = RealESRGANer(scale=netscale,model_path=model_path,dni_weight=dni_weight,model=model,tile=args.tile,tile_pad=args.tile_pad,pre_pad=args.pre_pad,half=False ,gpu_id=args.gpu_id,device='cpu')

    os.makedirs(args.output, exist_ok=True)
    if os.path.isfile(args.input):
        paths = [args.input]
    else:
        paths = sorted(glob.glob(os.path.join(args.input, '*')))

    for idx, path in enumerate(paths):
        imgname, extension = os.path.splitext(os.path.basename(path))
        print('Testing', idx, imgname)

        img = cv2.imread(path, cv2.IMREAD_UNCHANGED)
        if img is None:
            print(f'Error reading image: {imgname}')
            continue
        if len(img.shape) == 3 and img.shape[2] == 4:
            img_mode = 'RGBA'
        else:
            img_mode = None

        try:
            output, _ = upsampler.enhance(img, outscale=args.outscale)
        except RuntimeError as error:
            print('Error', error)
            print('If you encounter CUDA out of memory, try to set --tile with a smaller number.')
        else:
            if args.ext == 'auto':
                extension = extension[1:]
            else:
                extension = args.ext
            if img_mode == 'RGBA':
                extension = 'png'
            if args.suffix == '':
                save_path = os.path.join(args.output, f'{imgname}.{extension}')
            else:
                save_path = os.path.join(args.output, f'{imgname}_{args.suffix}.{extension}')
            cv2.imwrite(save_path, output)


if __name__ == '__main__':
    main()
