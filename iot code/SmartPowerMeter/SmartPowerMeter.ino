#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <WiFiManager.h> 
#include <ESP8266HTTPClient.h>
#include <WiFiClientSecureBearSSL.h>

String serverUrl = "https://smartpowermeter-dev.azurewebsites.net/EnergyMeasurement";
String deviceId = "1";


// Define configurações do Display OLED
#define OLED_RESET -1 
#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

// Define os pinos do sensor e a calibração do sensor
const int sensorPin = A0;
int mVperAmp = 185; 
float voltageStandard = 127;

// Variáveis para armazenar as medições
double voltage = 0;
double voltageRMS = 0;
double ampsRMS = 0;
float wattage = 0;
float kWh = 0;

unsigned long startTime = 0;
unsigned long elapsedTime = 0;

//WiFiManager, Local intialization. Once its business is done, there is no need to keep it around
WiFiManager wm;

void setup() {
  // Inicializa a comunicação serial
  Serial.begin(9600);
  
  // Inicializa o display OLED
  displaySetup();

  Wifisetup();

  // Configura o pino do sensor como entrada
  pinMode(sensorPin, INPUT);

  // Inicializa o tempo de início
  startTime = millis();
}

void loop() {
  // Imprime as medições no display OLED
  printMeasurements();

  // Calcula o tempo decorrido desde o último loop
  elapsedTime = millis() - startTime;

  // Realiza as medições de energia elétrica a cada minuto
  if (elapsedTime >= 60000) {
    // Imprime os valores de depuração no terminal serial
    printDebug();
    voltage = getVPP();
    voltageRMS = (voltage / 2.0) * 0.707;  // calcula o valor RMS
    ampsRMS = (voltageRMS * 1000) / mVperAmp;
    // Subtrai o valor de offset para obter a potência real consumida
    wattage = (voltageStandard * ampsRMS) - 21;
    // Calcula a energia consumida em kWh
    if (wattage >= 0) {
      kWh += (wattage / 1000.0) * (elapsedTime / 3600000.0);
    } else {
      wattage = 0;
    }

    postDataToApi();
    // Reinicia o tempo de início
    startTime = millis();
  }
}

void displaySetup() {
  display.begin(SSD1306_SWITCHCAPVCC, 0x3C); 
  display.display(); // mostra a tela inicial do Adafruit
  delay(2000);
  display.clearDisplay();
}

// Função que realiza a leitura do sensor de corrente
float getVPP() {
  float result = 0.0;

  int readValue;
  int maxValue = 0;
  int minValue = 1024; 

  uint32_t start_time = millis();

 // Faz a leitura do sensor por 1 segundo
  while ((millis() - start_time) < 1000) {
    readValue = analogRead(sensorPin);
    // Calcula o valor máximo e mínimo lidos
    if (readValue > maxValue) {
      maxValue = readValue;
    }
    if (readValue < minValue) {
      minValue = readValue;
    }
  }

 // Calcula o valor pico a pico da onda (peak-to-peak voltage)
  result = ((maxValue - minValue) * 5) / 1024.0;

  return result;
}

// Função que imprime o debug
void printDebug() {

  Serial.print("Voltage       : ");
  Serial.print(voltageStandard,0);
  Serial.println("V");
  Serial.print("Sensor Voltage: ");
  Serial.print(voltage, 8);
  Serial.println("V");
  Serial.print("Current.      : ");
  Serial.print(ampsRMS, 8);
  Serial.println("A RMS");
  Serial.print("Power.        : ");
  Serial.print(wattage, 8);
  Serial.println("W");
  Serial.print("Consumption.  : ");
  Serial.print(kWh, 8);
  Serial.println("kWh");
  Serial.println();
}

// Função que imprime os valores no display
void printMeasurements() {

  display.clearDisplay();
  display.setTextSize(1);           
  display.setTextColor(SSD1306_WHITE);  
  display.setCursor(0, 0);
  display.println("Smart Power Meter");    
  display.setCursor(0, 20);
  
  display.print(F("Voltage: "));
  display.print(voltageStandard,0);
  display.println(F("V"));

  display.print(F("Current: "));
  display.print(ampsRMS, 4);
  display.println(F("A"));

  display.print(F("Power  : "));
  display.print(wattage, 3);
  display.println(F("W"));

  display.print(F("Consum.: "));
  display.print(kWh, 5);
  display.println(F("kWh"));

  display.display();
}

void postDataToApi() {
  if (WiFi.status() == WL_CONNECTED) { 
    std::unique_ptr<BearSSL::WiFiClientSecure>client(new BearSSL::WiFiClientSecure);
    client->setInsecure();
    HTTPClient https;
    
    if (https.begin(*client, serverUrl)) {  
      Serial.println("[HTTPS] POST...");
  
      // Especificar o tipo de conteúdo da solicitação
      https.addHeader("Content-Type", "application/json");

      // Construir a string JSON
      String body = "{\"DeviceId\": \"" + deviceId + "\", \"Voltage\": " + String(voltageStandard) + ", \"SensorVoltage\": " + String(voltage, 8) + ", \"CurrentRMS\": " + String(ampsRMS, 8) + ", \"Power\": " + String(wattage, 8) + ", \"Consumption\": " + String(kWh, 8) + "}";

      // Enviar o POST request
      int httpCode = https.POST(body);

      if (httpCode > 0) {
        Serial.printf("[HTTPS] POST... code: %d\n", httpCode);
        if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_CREATED) {
          String payload = https.getString();
          Serial.println(String("[HTTPS] Received payload: ") + payload);
        }
      } else {
        Serial.printf("[HTTPS] POST... failed, error: %s\n\r", https.errorToString(httpCode).c_str());
      }

      https.end();
    } else {
      Serial.println("Error in WiFi connection");  
    }
  }
}



void Wifisetup()
{
  WiFi.mode(WIFI_STA);
  
  // reset settings - wipe stored credentials for testing
  // these are stored by the esp library
  //wm.resetSettings();

  bool res;
  res = wm.autoConnect("SmartPowerMeterWifi"); // anonymous ap
  if(!res) {
    Serial.println("Failed to connect Wifi");
    ESP.restart();
  } 
  else {
    Serial.println("Wifi Connected..");   
  }
}