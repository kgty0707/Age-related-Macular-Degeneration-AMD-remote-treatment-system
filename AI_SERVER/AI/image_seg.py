from ultralytics import YOLO
from PIL import Image
import torch

# 모델 처리
'''
user_name, ori_image_path, mask_image_path, sclera_x, sclera_y, cornea_x, cornea_y, created_dt
'''

model = YOLO('./AI/model/best.pt')

def get_segmentation_result(file_fath, user_name):
    '''
    return ori_image_path, mask_image_path, sclera_x, sclera_y, cornea_x, cornea_y
    '''
    result = model.predict(file_fath, save=True, imgsz=320, conf=0.5)
    boxes = result[0].boxes
    w, h = result[0].orig_shape
    width, height = culculate_lenght(boxes, w, h)
    im_bgr = result[0].plot()
    im_rgb = Image.fromarray(im_bgr[..., ::-1])  # RGB-order PIL image

    filename=f'./patient_res_image/{user_name}_result.jpg'

    result[0].save(filename=filename)

    return width, height, filename


def culculate_lenght(boxes, w, h):
    for box in boxes:
        c = box.cls
        if c[0] == 3: # 동공일때 만 좌표값 반환
            b = box.xyxy[0]
            x_min, y_min, x_max, y_max = b
            width = x_max - x_min
            height = y_max - y_min

            width = (width/w) * 10
            height = (height/h) * 10

            return width, height
        
def torch_ready():
    return print(torch.cuda.is_available())
        
torch_ready()