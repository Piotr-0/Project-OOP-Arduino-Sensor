const int trigPin = 9;
const int echoPin = 10;

void setup() {
  Serial.begin(9600);
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
}

void loop() {
  long duration;
  float distance;

  // Send a short pulse on the trigger pin
  digitalWrite(trigPin, LOW);
  delayMicroseconds(2);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);

  // Measure the time it takes for the pulse to return
  duration = pulseIn(echoPin, HIGH);

  // Calculate the distance in centimeters with one decimal place
  distance = (float)duration * 0.034 / 2;

  // Convert the distance to a string and replace '.' with ','
  String distanceString = String(distance, 1);
  distanceString.replace(".", ",");

  // Print the distance on the serial monitor
  Serial.println(distanceString);

  delay(400);
}
