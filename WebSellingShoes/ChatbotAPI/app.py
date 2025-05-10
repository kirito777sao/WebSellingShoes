# app.py

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from dotenv import load_dotenv
import os, re, requests, traceback
from database import get_db_connection

# 1) Load biến môi trường
load_dotenv()
GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY", "").strip()
USE_DB = bool(os.getenv("DATABASE_URL", "").strip())

# 2) Khởi tạo FastAPI + CORS
app = FastAPI(title="ChatbotAPI")
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:5000",
        "http://127.0.0.1:5000",
        "https://localhost:44383"
    ],
    allow_methods=["*"],
    allow_headers=["*"],
    allow_credentials=True,
)

# 3) Models
class ChatRequest(BaseModel):
    message: str
    user_id: int

class ChatResponse(BaseModel):
    reply: str

# 4) Fallback AI (Google Gemini)
def ai_generate(message: str) -> str:
    if not GOOGLE_API_KEY:
        return "AI key chưa được cấu hình."
    endpoint = (
        "https://generativelanguage.googleapis.com/v1beta/models/"
        f"gemini-2.0-flash:generateContent?key={GOOGLE_API_KEY}"
    )
    payload = {"contents":[{"parts":[{"text":message}]}]}
    try:
        r = requests.post(endpoint, json=payload)
        r.raise_for_status()
        cands = r.json().get("candidates", [])
        return cands[0]["content"]["parts"][0]["text"] if cands else "AI không có kết quả."
    except Exception as e:
        return f"AI lỗi: {str(e)}"

# 5) Ship
def query_shipping(full_address: str) -> str:
    if not USE_DB:
        return "Không thể truy vấn phí ship lúc này."
    parts = [p.strip() for p in full_address.split(",") if p.strip()]
    conn = get_db_connection(); cur = conn.cursor()
    cols = ["City", "District", "Ward"]
    try:
        for i, part in enumerate(parts):
            if i >= len(cols): break
            col = cols[i]
            cur.execute(f"SELECT TOP 1 Price FROM Shippings WHERE LOWER({col}) = ?", (part.lower(),))
            row = cur.fetchone()
            if row:
                return f"Phí ship đến {col} “{part.title()}”: {row[0]:,.0f} VND"
        return f"Chưa có phí ship cho địa chỉ “{full_address}”."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn phí ship: {str(e)}"
    finally:
        conn.close()

# 6) Coupon chung
def query_coupon_any() -> str:
    if not USE_DB:
        return "Không thể truy vấn coupon lúc này."
    try:
        conn = get_db_connection(); cur = conn.cursor()
        cur.execute("""
            SELECT TOP 1 Code, [Percent]
            FROM Coupons
            WHERE Status=1
              AND StartDate<=GETDATE() AND EndDate>=GETDATE()
            ORDER BY NEWID()
        """)
        row = cur.fetchone()
        if row:
            return f"Mã coupon: {row[0]} – Giảm {row[1]}%"
        return "Hiện không có coupon hợp lệ."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn coupon: {str(e)}"
    finally:
        conn.close()

# 7) Coupon cụ thể
def query_coupon_code(code: str) -> str:
    if not USE_DB:
        return "Không thể truy vấn coupon lúc này."
    try:
        conn = get_db_connection(); cur = conn.cursor()
        cur.execute("""
            SELECT Code, [Percent], Description
            FROM Coupons
            WHERE UPPER(Code)=? AND Status=1
              AND StartDate<=GETDATE() AND EndDate>=GETDATE()
        """, (code.upper(),))
        row = cur.fetchone()
        if row:
            return f"Có mã {row[0]} – Giảm {row[1]}% ({row[2]})"
        return f"Không tìm thấy coupon “{code}” hoặc đã hết hạn."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn coupon {code}: {str(e)}"
    finally:
        conn.close()

# 8) Top bán chạy
def query_top_selling(limit: int = 3) -> str:
    if not USE_DB:
        return "Không thể truy vấn top bán chạy."
    try:
        conn = get_db_connection(); cur = conn.cursor()
        cur.execute(f"SELECT TOP {limit} Name FROM Products ORDER BY Sold DESC")
        names = [r[0] for r in cur.fetchall()]
        return names and "Top bán chạy: " + ", ".join(names) or "Chưa có dữ liệu bán chạy."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn top bán chạy: {str(e)}"
    finally:
        conn.close()

# 9) Giá SP
def query_product_price(key: str) -> str:
    conn = get_db_connection(); cur = conn.cursor()
    try:
        if key.isdigit():
            cur.execute("SELECT Name, Price FROM Products WHERE Id=?", (int(key),))
        else:
            cur.execute("SELECT TOP 1 Name, Price FROM Products WHERE LOWER(Name) LIKE ?", (f"%{key.lower()}%",))
        row = cur.fetchone()
        if row:
            return f"{row[0]}: {row[1]:,.0f} VND"
        return f"Không tìm thấy sản phẩm “{key}”."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn giá SP: {str(e)}"
    finally:
        conn.close()

# 10) Size SP
def query_product_sizes(key: str) -> str:
    conn = get_db_connection(); cur = conn.cursor()
    try:
        if key.isdigit():
            cur.execute("SELECT Size FROM ProductSizes WHERE ProductId=?", (int(key),))
        else:
            cur.execute("""
                SELECT ps.Size
                FROM Products p
                JOIN ProductSizes ps ON p.Id=ps.ProductId
                WHERE LOWER(p.Name) LIKE ?
            """, (f"%{key.lower()}%",))
        sizes = [r[0] for r in cur.fetchall()]
        return sizes and f"Sizes: {', '.join(sizes)}" or f"Chưa có size cho “{key}”."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn size: {str(e)}"
    finally:
        conn.close()

# 11) Tồn kho SP
def query_product_stock(key: str) -> str:
    if not key.isdigit():
        return "Vui lòng cho biết ID sản phẩm để kiểm tra tồn kho."
    conn = get_db_connection(); cur = conn.cursor()
    try:
        cur.execute("SELECT Stock FROM Products WHERE Id=?", (int(key),))
        row = cur.fetchone()
        if row:
            return f"Tồn kho SP #{key}: {row[0]}"
        return f"Không tìm thấy SP #{key}."
    except Exception as e:
        traceback.print_exc()
        return f"Lỗi truy vấn tồn kho: {str(e)}"
    finally:
        conn.close()

# 12) Endpoint /chat
@app.post("/chat", response_model=ChatResponse)
async def chat(req: ChatRequest):
    txt = req.message.strip()
    low = txt.lower()

    # -- Phí ship --
    if "ship" in low:
        m = re.search(r"(?:phí\s*ship|ship)\s*(.*)", txt, re.IGNORECASE)
        return {"reply": query_shipping(m.group(1).strip())} if m else {"reply": "Chưa rõ địa chỉ ship."}

    # -- Coupon cụ thể --
    m = re.search(r"(?:mã giảm giá|coupon)\s+([A-Za-z0-9]+)", txt, re.IGNORECASE)
    if m:
        return {"reply": query_coupon_code(m.group(1))}

    # -- Coupon chung --
    if re.search(r"\b(mã giảm giá|coupon)\b", low):
        return {"reply": query_coupon_any()}

    # -- Top bán chạy --
    if re.search(r"(bán chạy|top bán chạy)", low):
        return {"reply": query_top_selling()}

    # -- Giá SP --
    m = re.search(r"gi[áa]\s*sản\s*phẩm\s+(.+)", txt, re.IGNORECASE)
    if m:
        return {"reply": query_product_price(m.group(1).strip())}

    # -- Size SP --
    m = re.search(r"size\s*sản\s*phẩm\s+(.+)", txt, re.IGNORECASE)
    if m:
        return {"reply": query_product_sizes(m.group(1).strip())}

    # -- Tồn kho SP --
    m = re.search(r"tồn\s*kho\s*sản\s*phẩm\s+(\d+)", low)
    if m:
        return {"reply": query_product_stock(m.group(1))}

    # -- Fallback AI --
    return {"reply": ai_generate(txt)}

# 13) Health-check
@app.get("/health")
def health():
    return {"status": "ok"}
