from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker

from dotenv import load_dotenv
import os

load_dotenv()
DB_URL = os.getenv("DATABASE_URL")

class engineconn:

    def __init__(self):
        self.engine = create_engine(DB_URL, pool_recycle = 500)

    def get_session(self):
        SessionLocal = sessionmaker(bind=self.engine)
        return SessionLocal()

    def get_connection(self):
        return self.engine.connect()