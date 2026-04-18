import os
import torch
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from transformers import AutoTokenizer, AutoModelForSequenceClassification
from langdetect import detect, DetectorFactory, LangDetectException

DetectorFactory.seed = 0

MODEL_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "model", "final")

app = FastAPI(title="Text Moderation API")

model = None
tokenizer = None
device = torch.device("cpu")


class PredictRequest(BaseModel):
    text: str
    threshold: float = 0.85


class PredictResponse(BaseModel):
    is_toxic: bool
    score: float
    language: str


@app.on_event("startup")
def load_model():
    global model, tokenizer
    tokenizer = AutoTokenizer.from_pretrained(MODEL_DIR)
    model = AutoModelForSequenceClassification.from_pretrained(MODEL_DIR)
    model.to(device)
    model.eval()


@app.get("/health")
def health():
    return {"status": "ok"}


@app.post("/predict", response_model=PredictResponse)
def predict(req: PredictRequest):
    text = (req.text or "").strip()
    if not text:
        raise HTTPException(status_code=400, detail="Text must not be empty")

    try:
        language = detect(text)
    except LangDetectException:
        language = "unknown"

    inputs = tokenizer(
        text,
        return_tensors="pt",
        truncation=True,
        padding=True,
        max_length=128,
    ).to(device)

    with torch.no_grad():
        outputs = model(**inputs)
        logits = outputs.logits if hasattr(outputs, "logits") else outputs[0]
        probs = torch.softmax(logits, dim=-1)
        toxic_score = float(probs[0, 1].item())
    
    return PredictResponse(
        is_toxic=toxic_score >= req.threshold,
        score=toxic_score,
        language=language,
    )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001)
