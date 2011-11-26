// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
 * @name: S11.4.8_A2.2_T1;
 * @section: 11.4.8, 8.6.2.6;
 * @assertion: Operator ~x uses [[Default Value]];
 * @description: If Type(value) is Object, evaluate ToPrimitive(value, Number);
 */

//CHECK#1
var object = {valueOf: function() {return 1}};
if (~object !== -2) {
  $ERROR('#1: var object = {valueOf: function() {return 1}}; ~object === -2. Actual: ' + (~object));
}

//CHECK#2
var object = {valueOf: function() {return 1}, toString: function() {return 0}};
if (~object !== -2) {
  $ERROR('#2: var object = {valueOf: function() {return 1}, toString: function() {return 0}}; ~object === -2. Actual: ' + (~object));
} 

//CHECK#3
var object = {valueOf: function() {return 1}, toString: function() {return {}}};
if (~object !== -2) {
  $ERROR('#3: var object = {valueOf: function() {return 1}, toString: function() {return {}}}; ~object === -2. Actual: ' + (~object));
}

//CHECK#4
try {
  var object = {valueOf: function() {return 1}, toString: function() {throw "error"}};
  if (~object !== -2) {
    $ERROR('#4.1: var object = {valueOf: function() {return 1}, toString: function() {throw "error"}}; ~object === -2. Actual: ' + (~object));
  }
}
catch (e) {
  if (e === "error") {
    $ERROR('#4.2: var object = {valueOf: function() {return 1}, toString: function() {throw "error"}}; ~object not throw "error"');
  } else {
    $ERROR('#4.3: var object = {valueOf: function() {return 1}, toString: function() {throw "error"}}; ~object not throw Error. Actual: ' + (e));
  }
}

//CHECK#5
var object = {toString: function() {return 1}};
if (~object !== -2) {
  $ERROR('#5: var object = {toString: function() {return 1}}; ~object === -2. Actual: ' + (~object));
}

//CHECK#6
var object = {valueOf: function() {return {}}, toString: function() {return 1}}
if (~object !== -2) {
  $ERROR('#6: var object = {valueOf: function() {return {}}, toString: function() {return 1}}; ~object === -2. Actual: ' + (~object));
}

//CHECK#7
try {
  var object = {valueOf: function() {throw "error"}, toString: function() {return 1}};
  ~object;
  $ERROR('#7.1: var object = {valueOf: function() {throw "error"}, toString: function() {return 1}}; ~object throw "error". Actual: ' + (~object));
}  
catch (e) {
  if (e !== "error") {
    $ERROR('#7.2: var object = {valueOf: function() {throw "error"}, toString: function() {return 1}}; ~object throw "error". Actual: ' + (e));
  } 
}

//CHECK#8
try {
  var object = {valueOf: function() {return {}}, toString: function() {return {}}};
  ~object;
  $ERROR('#8.1: var object = {valueOf: function() {return {}}, toString: function() {return {}}}; ~object throw TypeError. Actual: ' + (~object));
}  
catch (e) {
  if ((e instanceof TypeError) !== true) {
    $ERROR('#8.2: var object = {valueOf: function() {return {}}, toString: function() {return {}}}; ~object throw TypeError. Actual: ' + (e));
  } 
}
