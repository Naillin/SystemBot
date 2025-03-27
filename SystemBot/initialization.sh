#!/bin/bash

# ��� �������
SERVICE_NAME="system-bot.service"

# �������� ������� ����������, ��� ����������� ������
CURRENT_DIR=$(pwd)

# ���������, ���������� �� ������
if systemctl list-unit-files | grep -q "^$SERVICE_NAME"; then
	echo "Service $SERVICE_NAME already exists. Stopping and disabling it..."
	
	# ������������� ������
	sudo systemctl stop $SERVICE_NAME
	
	# ��������� ������
	sudo systemctl disable $SERVICE_NAME
	
	# ������� ���� �������
	sudo rm -f /etc/systemd/system/$SERVICE_NAME
	
	# ������������� systemd
	sudo systemctl daemon-reload
	
	echo "Service $SERVICE_NAME has been stopped and removed."
fi

# ������� ���������� ��� .service �����
SERVICE_CONTENT="[Unit]
Description=System Bot
After=network.target

[Service]
ExecStart=$CURRENT_DIR/MQTT_Rules
WorkingDirectory=$CURRENT_DIR
Restart=always
User=root
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target"

# ������� .service ���� � /etc/systemd/system/
echo "$SERVICE_CONTENT" | sudo tee /etc/systemd/system/$SERVICE_NAME > /dev/null

# ������������� systemd ��� ���������� ���������
sudo systemctl daemon-reload

# �������� ������
#sudo systemctl enable $SERVICE_NAME

# ��������� ������
sudo systemctl start $SERVICE_NAME

echo "Service $SERVICE_NAME has been created and started."
echo "If you need enable service for autorun. Execute 'sudo systemctl enable $SERVICE_NAME'."
