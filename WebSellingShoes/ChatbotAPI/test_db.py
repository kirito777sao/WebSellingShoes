# ChatbotAPI/test_db.py
from database import get_db_connection

def main():
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute("SELECT TOP 1 Name FROM Products")
    row = cur.fetchone()
    print("Fetch product:", row[0] if row else "no data")
    conn.close()

if __name__ == "__main__":
    main()
