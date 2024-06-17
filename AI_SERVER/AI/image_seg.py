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
    주어진 이미지 파일 경로와 사용자 이름을 받아, 이미지에서 동공을 탐지하고 결과 이미지를 저장한 후,
    각막의 가로 및 세로 길이와 결과 이미지의 경로를 반환하는 함수

    Parameters:
    file_fath (str): 입력 이미지 파일 경로
    user_name (str): 사용자 이름

    Returns:
    width (float): 각막의 가로 길이 (상대적 길이)
    height (float): 각막의 세로 길이 (상대적 길이)
    filename (str): 결과 이미지 파일 경로
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
    '''
    바운딩 박스 정보를 받아 동공의 좌표값을 계산하고, 이미지 크기에 비례한 가로 및 세로 길이를 반환하는 함수

    Parameters:
    boxes (list): 바운딩 박스 리스트
    w (int): 원본 이미지의 가로 길이
    h (int): 원본 이미지의 세로 길이

    Returns:
    width (float): 동공의 가로 길이 (상대적 길이)
    height (float): 동공의 세로 길이 (상대적 길이)
    '''
    for box in boxes:
        c = box.cls
        if c[0] == 3: 
            b = box.xyxy[0]
            x_min, y_min, x_max, y_max = b
            width = x_max - x_min
            height = y_max - y_min

            width = (width/w) * 10
            height = (height/h) * 10

            return width, height