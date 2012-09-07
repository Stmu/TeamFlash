# TeamFlash

TeamFlash is a super simple tool for connecting TeamCity to a build light that can run on a PC or on your Raspberry Pi!

It's tested with the [Delcom 904003](http://www.delcomproducts.com/productdetails.asp?productnum=904003) and [Delcom 804003](http://www.delcomproducts.com/productdetails.asp?productnum=804003) (old generation).

If you're on the Pi it can use LEDs hooked up to the GPIO pins.

## Installing

1. Grab [the compiled bits, hot off our build server](https://tc.readifycloud.com/viewLog.html?buildTypeId=bt10&buildId=lastSuccessful&tab=artifacts&guest=1)
2. Run them
3. There is no installer

## Outputs

### USB Light

<dl>
  <dt>Red</dt>
  <dd>one or more builds are broken, and not being investigated</dd>
  <dt>Amber</dt>
  <dd>all builds are either under investigation (requires TeamCity 7.1), or passing</dd>
  <dt>Green</dt>
  <dd>all builds are passing</dd>
</dl>

### GPIO LEDs

LED - Pin Assignments:
<dl>
  <dt>GPIO 00</dt>
  <dd>Green</dd>
  <dt>GPIO 01</dt>
  <dd>Red</dd>
  <dt>GPIO 04</dt>
  <dd>Yello</dd>
</dl>

States:
<dl>
  <dt>Red (solid)</dt>
  <dd>one or more builds are broken, and not being investigated</dd>
  <dt>Red - Yellow flashing</dt>
  <dd>all builds are either under investigation (requires TeamCity 7.1), or passing</dd>
  <dt>Green</dt>
  <dd>all builds are passing</dd>
  <dt>Red (flashing)</dt>
  <dd>the build service is unavailable or there was an error with communications</dd>
  <dt>Red + Green + Yellow (solid)</dt>
  <dd>something really funky is going on...</dd>
</dl>

Paused builds are ignored.

## Code

…is moderately average. We know. We hacked it together on a client site eons ago, then published it when we wanted to re-use it elsewhere. Don't hate us: send a pull request instead.

We also know that there are other projects that do the same thing, like [SouthsideSoftware/TeamCityBuildLight](https://github.com/SouthsideSoftware/TeamCityBuildLight). We wrote this a while ago, and wanted to publish it anyway.
