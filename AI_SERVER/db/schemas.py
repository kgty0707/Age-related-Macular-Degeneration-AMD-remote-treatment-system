from pydantic import BaseModel
from datetime import datetime

class ResultCreate(BaseModel):
    user_name: str
    ori_image_path: str
    mask_image_path: str
    sclera_x: str
    sclera_y: str
    cornea_x: str
    cornea_y: str
    created_dt: datetime

    class Config:
        orm_mode = True