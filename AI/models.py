from sqlalchemy import create_engine, Column, Integer, String, DateTime
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()

class Test(Base):
    __tablename__ = 'result_table'

    result_idx = Column(Integer, primary_key=True)
    user_name = Column(String(64))
    ori_image_path = Column(String(512))
    mask_image_path = Column(String(512))
    sclera_x = Column(String(45))
    sclera_y = Column(String(45))
    cornea_x = Column(String(45))
    cornea_y = Column(String(45))
    created_dt = Column(DateTime)