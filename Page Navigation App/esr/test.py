import os
import cv2
import numpy as np
import torch
import RRDBNet_arch as arch

def super_resolve_image(input_image_path, output_folder='results', model_path='models/RRDB_ESRGAN_x4.pth'):
    device = torch.cuda.current_device() if torch.cuda.is_available() else torch.device('cpu')
    model = arch.RRDBNet(3, 3, 64, 23, gc=32)
    model.load_state_dict(torch.load(model_path), strict=True)
    model.eval()
    model = model.to(device)
    base = os.path.splitext(os.path.basename(input_image_path))[0]
    img = cv2.imread(input_image_path, cv2.IMREAD_COLOR)
    img = img * 1.0 / 255
    img = torch.from_numpy(np.transpose(img[:, :, [2, 1, 0]], (2, 0, 1))).float()
    img_LR = img.unsqueeze(0)
    img_LR = img_LR.to(device)
    with torch.no_grad():
        output = model(img_LR).data.squeeze().float().cpu().clamp_(0, 1).numpy()
    output = np.transpose(output[[2, 1, 0], :, :], (1, 2, 0))
    output = (output * 255.0).round()
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)
    output_path = os.path.join(output_folder, f'{base}.png')
    cv2.imwrite(output_path, output)
    return output_path

super_resolve_image('input/test.jpg')