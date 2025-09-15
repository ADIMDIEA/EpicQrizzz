import sqlite3
import pandas as pd
import re

# Lees het MySQL dump bestand
with open("kennisquiz.sql", "r", encoding="utf-8") as f:
    sql_script = f.read()

# Verwijder MySQL-specifieke commando's die SQLite niet begrijpt
patterns = [
    r"SET .*?;\n",
    r"START TRANSACTION;",
    r"COMMIT;",
    r"/\*![0-9]+ .*?\*/;",
    r"ENGINE=.*?;",
    r"AUTO_INCREMENT=\d+",
    r"CHARSET=.*?(;|\n)"
]

for pat in patterns:
    sql_script = re.sub(pat, "", sql_script, flags=re.IGNORECASE)

# Maak verbinding met SQLite in-memory
conn = sqlite3.connect(":memory:")

# Voer het opgeschoonde SQL script uit
conn.executescript(sql_script)

# Query om de vragen en keuzes in jouw format te zetten
query = """
WITH opts AS (
    SELECT
        q.id AS Id,
        q.question_text AS Question,
        c.identifier,
        c.choice_text,
        c.is_correct
    FROM questions q
    LEFT JOIN choices c ON q.id = c.question_id
)
SELECT
    Id,
    Question,
    MAX(CASE WHEN identifier = 'A1' THEN choice_text END) AS OptionA,
    MAX(CASE WHEN identifier = 'A2' THEN choice_text END) AS OptionB,
    MAX(CASE WHEN identifier = 'A3' THEN choice_text END) AS OptionC,
    MAX(CASE WHEN identifier = 'A4' THEN choice_text END) AS OptionD,
    CASE
        WHEN MAX(CASE WHEN identifier = 'A1' AND is_correct = 1 THEN 'A' END) IS NOT NULL THEN 'A'
        WHEN MAX(CASE WHEN identifier = 'A2' AND is_correct = 1 THEN 'B' END) IS NOT NULL THEN 'B'
        WHEN MAX(CASE WHEN identifier = 'A3' AND is_correct = 1 THEN 'C' END) IS NOT NULL THEN 'C'
        WHEN MAX(CASE WHEN identifier = 'A4' AND is_correct = 1 THEN 'D' END) IS NOT NULL THEN 'D'
    END AS CorrectOption
FROM opts
GROUP BY Id, Question
ORDER BY Id;
"""

df = pd.read_sql_query(query, conn)

# Bekijk de eerste regels
print(df.head())

# Export naar CSV
df.to_csv("quiz_export.csv", index=False, encoding="utf-8")
