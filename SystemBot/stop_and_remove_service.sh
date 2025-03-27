#!/bin/bash

# ��� �������
SERVICE_NAME="system-bot.service"

# ��������� �������, ���� �� �������
if systemctl is-active --quiet "$SERVICE_NAME"; then
	echo "Stopping $SERVICE_NAME..."
	sudo systemctl stop "$SERVICE_NAME"
	echo "$SERVICE_NAME stopped."
else
	echo "$SERVICE_NAME is not running."
fi

# ���������� �������, ���� �� �������
if systemctl is-enabled --quiet "$SERVICE_NAME"; then
	echo "Disabling $SERVICE_NAME..."
	sudo systemctl disable "$SERVICE_NAME"
	echo "$SERVICE_NAME disabled."
else
	echo "$SERVICE_NAME is not enabled."
fi

# �������� ����� �������, ���� �� ����������
SERVICE_FILE="/etc/systemd/system/$SERVICE_NAME"
if [ -f "$SERVICE_FILE" ]; then
	echo "Removing $SERVICE_FILE..."
	sudo rm -f "$SERVICE_FILE"
	echo "$SERVICE_FILE removed."
else
	echo "$SERVICE_FILE does not exist."
fi

# ������������ systemd ��� ���������� ���������
sudo systemctl daemon-reload
echo "Systemd daemon reloaded."

echo "Service $SERVICE_NAME has been stopped and removed."
