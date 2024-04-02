from fastapi import FastAPI, Depends, Path, HTTPException
from pydantic import BaseModel
from database import engineconn
from models import Test

app = FastAPI()

engine = engineconn()
session = engine.sessionmaker()

class Item(BaseModel):
    name: str
    number: int

@app.get("/")
async def first_get():
    example = session.query(Test).all()
    return example

@app.post("/create-item")
async def create_item(item: Item):
    print(item.name)
    print(item.number)
    return {"message": "Item received", "name": item.name, "number": item.number}


@app.get("/get-message/")
async def get_message():
    return {"message": "Hello from FastAPI!"}