from ultralytics import YOLO
from PIL import Image
import supervision

# 모델 처리
'''
user_name, ori_image_path, mask_image_path, sclera_x, sclera_y, cornea_x, cornea_y, created_dt
'''

model = YOLO('./runs/detect/train93/weights/best.pt')

def get_segmentation_result(file_fath):
    '''
    return ori_image_path, mask_image_path, sclera_x, sclera_y, cornea_x, cornea_y
    '''
    result = model.predict(file_fath, save=True, imgsz=320, conf=0.5)
    ...