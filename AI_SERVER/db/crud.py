from sqlalchemy.orm import Session
from sqlalchemy import desc
from . import models, schemas

def insert_data(db: Session, result_create: schemas.ResultCreate):
    db_item = models.ResultTable(
        user_name=result_create.user_name,
        ori_image_path=result_create.ori_image_path,
        mask_image_path=result_create.mask_image_path,
        sclera_x=result_create.sclera_x,
        sclera_y=result_create.sclera_y,
        cornea_x=result_create.cornea_x,
        cornea_y=result_create.cornea_y,
        created_dt=result_create.created_dt
    )
    db.add(db_item)
    db.commit()
    db.refresh(db_item)
    return db_item

def get_all_results(db: Session):
    return db.query(models.ResultTable).all()

def get_latest_result(db: Session):
    return db.query(models.ResultTable).order_by(desc(models.ResultTable.created_dt)).first()