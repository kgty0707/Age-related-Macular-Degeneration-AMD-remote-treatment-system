from datetime import datetime
import os
from fastapi.templating import Jinja2Templates
from fastapi.staticfiles import StaticFiles

from fastapi import FastAPI, Depends, Request
from typing import List, Optional
from sqlalchemy.orm import Session
from pydantic import BaseModel
from db.database import engineconn
from db.models import ResultTable
from db import crud, schemas
from AI.image_seg import get_segmentation_result

app = FastAPI()
app.mount("/static", StaticFiles(directory="static"), name="static")
app.mount("/patient_res_image", StaticFiles(directory="patient_res_image"), name="patient_res_image")
app.mount("/patient_ori_image", StaticFiles(directory="patient_ori_image"), name="patient_ori_image")

templates = Jinja2Templates(directory="templates")

engine = engineconn()

def get_db():
    try:
        db = engine.get_session()
        yield db
    finally:
        db.close()

@app.get("/main")
def hello(request: Request, db: Session = Depends(get_db)):
    example = db.query(ResultTable).all()
    return templates.TemplateResponse("main.html", {"request": request, "example": example})

@app.get("/")
def first_get(db: Session = Depends(get_db)):
    """
    DB에서 모든 데이터 조회 후 반환
    """
    example = db.query(ResultTable).all()
    return example

@app.get("/get-message/")
def get_message():
    """
    Unity에서 테스트 메세지 확인용
    """
    return {"message": "Hello from FastAPI!"}

# @app.post("/create-dummy-data/")
# def create_dummy_data(request_data: schemas.ResultCreate, db: Session = Depends(get_db)):
#     """
#     unity에서 더미 데이터 만든 후 db insert 함수
#     """
#     crud.insert_data(db=db, result_create=request_data)
#     return {"message": "Item received", "name": request_data.user_name, "number": request_data.created_dt}

@app.get("/results/", response_model=List[schemas.ResultCreate])
def read_results(db: Session = Depends(get_db)):
    """
    db에서 모든 data 조회 후 unity에 반환
    """
    results = crud.insert_all_results(db=db)
    return results

@app.get("/results/latest", response_model=schemas.ResultCreate)
def read_latest_result(db: Session = Depends(get_db)):
    """
    db에서 created_dt를 기준으로 최근 1개의 데이터 반환
    """
    latest_result = crud.insert_latest_results(db=db)
    return latest_result

class ImportImageResponse(BaseModel):
    message: str
    width: float
    height: float

file_path = './patient_ori_image/minju_eye.jpg'

@app.get("/import_image", response_model=ImportImageResponse)
def import_image(db: Session = Depends(get_db)):
    user_name = os.path.basename(file_path).split('_')[0]
    width, height, result_path = get_segmentation_result(file_path, user_name)
    
    created_dt = datetime.now()

    data = schemas.ResultCreate(
        user_name=user_name,
        ori_image_path=file_path,  # Original image path
        mask_image_path=result_path,         # Mask image path
        sclera_x=str(width), sclera_y=str(height),  # Sclera center coordinates as strings
        cornea_x="0", cornea_y="0",  # Cornea center coordinates as strings
        created_dt=created_dt  # Timestamp
    )

    # Assuming crud.insert_data properly saves the data to the database
    db_item = crud.insert_data(db=db, result_create=data)
    
    # Return simple confirmation and metrics
    return ImportImageResponse(message="Item received", width=float(width), height=float(height))