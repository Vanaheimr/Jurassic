// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S15.8.2.17_A3;
 * @section: 15.8.2.17;
 * @assertion: If x is equal to +0, Math.sqrt(x) is +0;
 * @description: Checking if Math.sqrt(+0) equals to +0;
 */
 
// CHECK#1
var x = +0;
if (Math.sqrt(x) !== +0)
{
	$ERROR("#1: 'var x=+0; Math.sqrt(x) !== +0'");
}
