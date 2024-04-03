from fastapi import FastAPI, Depends, Path, HTTPException
from typing import List, Optional
from sqlalchemy.orm import Session
from pydantic import BaseModel
from db.database import engineconn
from db.models import ResultTable
from db import crud, schemas

app = FastAPI()

engine = engineconn()

def get_db():
    try:
        db = engine.get_session()
        yield db
    finally:
        db.close()

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

@app.post("/create-dummy-data/")
def create_dummy_data(request_data: schemas.ResultCreate, db: Session = Depends(get_db)):
    """
    unity에서 더미 데이터 만든 후 db insert 함수
    """
    crud.insert_data(db=db, result_create=request_data)
    return {"message": "Item received", "name": request_data.user_name, "number": request_data.created_dt}

@app.get("/results/", response_model=List[schemas.ResultCreate])
def read_results(db: Session = Depends(get_db)):
    """
    db에서 모든 data 조회 후 unity에 반환
    """
    results = crud.get_all_results(db=db)
    return results

@app.get("/results/latest", response_model=schemas.ResultCreate)
def read_latest_result(db: Session = Depends(get_db)):
    """
    db에서 created_dt를 기준으로 최근 1개의 데이터 반환
    """
    latest_result = crud.get_latest_result(db=db)
    return latest_result