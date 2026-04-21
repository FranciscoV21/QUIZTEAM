from fastapi import FastAPI, Query
from pydantic import BaseModel
import mysql.connector

app = FastAPI()

# =========================
# CONFIG DB
# =========================
db_config = {
    "host": "127.0.0.1",
    "user": "root",
    "password": "",
    "database": "quiz"
}

def get_connection():
    return mysql.connector.connect(**db_config)

# =========================
# MODELOS (para POST)
# =========================
class Respuesta(BaseModel):
    id_pregunta: int
    respuesta: int

# =========================
# ROOT
# =========================
@app.get("/")
def root():
    return {"message": "API funcionando 🚀"}

# =========================
# TEST DB
# =========================
@app.get("/test-db")
def test_db():
    try:
        conn = get_connection()
        conn.close()
        return {"message": "Conexión a MySQL exitosa ✅"}
    except Exception as e:
        return {"error": str(e)}

# =========================
# CATEGORIAS
# =========================
@app.get("/categorias")
def get_categorias():
    try:
        conn = get_connection()
        cursor = conn.cursor(dictionary=True)

        cursor.execute("SELECT id, nombre FROM categorias")
        data = cursor.fetchall()

        cursor.close()
        conn.close()

        return data

    except Exception as e:
        return {"error": str(e)}

# =========================
# PREGUNTAS POR CATEGORIA
# =========================
@app.get("/preguntas")
def get_preguntas(categoria: str = Query(...)):
    try:
        conn = get_connection()
        cursor = conn.cursor(dictionary=True)

        query = """
        SELECT p.*
        FROM preguntas p
        JOIN categorias c ON p.categoria_id = c.id
        WHERE c.nombre = %s
        ORDER BY RAND()
        LIMIT 10
        """

        cursor.execute(query, (categoria,))
        data = cursor.fetchall()

        cursor.close()
        conn.close()

        return data

    except Exception as e:
        return {"error": str(e)}

# =========================
# VALIDAR RESPUESTA
# =========================
@app.post("/responder")
def responder(respuesta: Respuesta):
    try:
        conn = get_connection()
        cursor = conn.cursor(dictionary=True)

        cursor.execute(
            "SELECT correcta FROM preguntas WHERE id = %s",
            (respuesta.id_pregunta,)
        )

        result = cursor.fetchone()

        cursor.close()
        conn.close()

        if result is None:
            return {"error": "Pregunta no encontrada"}

        if result["correcta"] == respuesta.respuesta:
            return {
                "correcto": True,
                "puntos": 10
            }
        else:
            return {
                "correcto": False,
                "puntos": 0
            }

    except Exception as e:
        return {"error": str(e)}

# =========================
# GUARDAR PARTIDA
# =========================
class Partida(BaseModel):
    categoria: str
    correctas: int
    total: int

@app.post("/guardar-partida")
def guardar_partida(partida: Partida):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        query = """
        INSERT INTO partidas (categoria, correctas, total)
        VALUES (%s, %s, %s)
        """

        cursor.execute(query, (
            partida.categoria,
            partida.correctas,
            partida.total
        ))

        conn.commit()

        cursor.close()
        conn.close()

        return {"message": "Partida guardada correctamente"}

    except Exception as e:
        return {"error": str(e)}