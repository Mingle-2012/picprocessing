import torch

if __name__ == '__main__':

    model_path = 'RealESRGAN_x4plus.pth'
    model_dict = torch.load(model_path)

    print(model_dict.keys())