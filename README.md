# Project: SNT-SS22
## About
### SlowsieNT-SilverSpark*22*
*"Http Silver Spark is supposed to be the cutest, and tiny spark; Allows many customizations!"*<br>

## Notes
If possible (NOT required): include credits, thank you!

## Requirements
.NET 3.5+

## Example:
```cs
HttpSilverSpark.Load("useragents.txt");
  var vSpark = new HttpSilverSpark("localhost", 81) {
  Body = "msg#05=#1a",   // magickal string to be converted to random string
  Method = "POST",       // post usually returns status 200, regardless if target is https
  ReceiveDataLength = 44 // unnecessary
};
vSpark.Headers.Add("Content-Type: application/x-www-form-urlencoded");
vSpark.OnSent += VSpark_OnSent;
vSpark.OnReceive += VSpark_OnReceive;
vSpark.OnError += VSpark_OnError;
// Spark can be repeated
vSpark.Spark();
```
