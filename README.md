# SNT-SS22

.NET 3.5+

Example code:<br>
```cs
HttpSilverSpark.Load("useragents.txt");
  var vSpark = new HttpSilverSpark("localhost", 81) {
  Body = "msg#05=#1a",   // magickal string to be converted to random string
  Method = "POST",       // post usually returns status 200, regardless if target is https
  ReceiveDataLength = 11 // unnecessary
};
vSpark.Headers.Add("Content-Type: application/x-www-form-urlencoded");
vSpark.OnSent += VSpark_OnSent;
vSpark.OnReceive += VSpark_OnReceive;
vSpark.OnError += VSpark_OnError;
// Spark can be repeated
vSpark.Spark();
```
