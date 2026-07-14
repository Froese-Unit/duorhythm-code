// This script sends TTL pulses continuously to the EEG PC via BNC and "PULSE" message to Unity PC via Serial USB (COMPORT).

void setup() {
  pinMode(2, OUTPUT);
  Serial.begin(9600);
}

void loop() {
  digitalWrite(2, HIGH);
  Serial.println("PULSE");  // Unity reads this
  delay(10);                // 10ms pulse width
  digitalWrite(2, LOW);
  delay(2000);              // wait 2 seconds
}