# ChatbotAPI/database.py
import os, pyodbc
from dotenv import load_dotenv

# 1. Load .env
load_dotenv()
CONN_STR = os.getenv("DATABASE_URL", "").strip()
if not CONN_STR:
    raise RuntimeError("Missing DATABASE_URL in .env")

# 2. Trả về kết nối ODBC
def get_db_connection():
    try:
        conn = pyodbc.connect(CONN_STR, autocommit=True)
        return conn
    except Exception as e:
        print(f"[database.py] Lỗi kết nối CSDL: {e}")
        raise
