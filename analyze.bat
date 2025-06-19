@echo off
cd Analytics || exit /b
pip install -r requirements.txt
python analyze.py
