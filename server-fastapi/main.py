from fastapi import FastAPI, Query, WebSocket, WebSocketDisconnect
from pydantic import BaseModel
import mysql.connector
from typing import Dict, List
import uuid

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
# MODELOS
# =========================
class Respuesta(BaseModel):
    id_pregunta: int
    respuesta: int

class Partida(BaseModel):
    categoria: str
    correctas: int
    total: int

# =========================
# MULTIJUGADOR
# =========================
rooms: Dict[str, List[WebSocket]] = {}
scores: Dict[str, Dict[str, dict]] = {}

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
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    cursor.execute("SELECT id, nombre FROM categorias")
    data = cursor.fetchall()

    cursor.close()
    conn.close()

    return data

# =========================
# PREGUNTAS
# =========================
@app.get("/preguntas")
def get_preguntas(categoria: str = Query(...)):
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

# =========================
# RESPONDER
# =========================
@app.post("/responder")
def responder(respuesta: Respuesta):
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

    correcto = result["correcta"] == respuesta.respuesta

    return {
        "correcto": correcto,
        "puntos": 10 if correcto else 0
    }

# =========================
# GUARDAR PARTIDA
# =========================
@app.post("/guardar-partida")
def guardar_partida(partida: Partida):
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

# =========================
# 🔥 WEBSOCKET MULTIJUGADOR PRO
# =========================
@app.websocket("/ws/{room_id}")
async def websocket_endpoint(websocket: WebSocket, room_id: str):
    await websocket.accept()

    # Crear sala si no existe
    if room_id not in rooms:
        rooms[room_id] = []
        scores[room_id] = {}

    rooms[room_id].append(websocket)

    player_id = str(uuid.uuid4())

    # 🔥 Jugador numerado
    player_number = len(scores[room_id]) + 1

    scores[room_id][player_id] = {
        "nombre": f"Jugador {player_number}",
        "puntos": 0,
        "correctas": 0,
        "incorrectas": 0
    }

    # Enviar info al jugador
    await websocket.send_json({
        "type": "connected",
        "player_id": player_id,
        "nombre": scores[room_id][player_id]["nombre"]
    })

    try:
        while True:
            data = await websocket.receive_json()

            # =========================
            # RESPUESTA
            # =========================
            if data.get("type") == "respuesta":
                puntos = data.get("puntos", 0)

                scores[room_id][player_id]["puntos"] += puntos

                if puntos > 0:
                    scores[room_id][player_id]["correctas"] += 1
                else:
                    scores[room_id][player_id]["incorrectas"] += 1

                # Ranking ordenado
                ranking = sorted(
                    scores[room_id].values(),
                    key=lambda x: x["puntos"],
                    reverse=True
                )

                for conn in rooms[room_id]:
                    await conn.send_json({
                        "type": "score",
                        "ranking": ranking
                    })

            # =========================
            # INICIO
            # =========================
            elif data.get("type") == "start":
                for conn in rooms[room_id]:
                    await conn.send_json({
                        "type": "start"
                    })

            # =========================
            # FINAL
            # =========================
            elif data.get("type") == "end":
                ranking = sorted(
                    scores[room_id].values(),
                    key=lambda x: x["puntos"],
                    reverse=True
                )

                for conn in rooms[room_id]:
                    await conn.send_json({
                        "type": "final",
                        "ranking": ranking
                    })

    except WebSocketDisconnect:
        rooms[room_id].remove(websocket)

        if player_id in scores[room_id]:
            del scores[room_id][player_id]

        if len(rooms[room_id]) == 0:
            del rooms[room_id]
            del scores[room_id]