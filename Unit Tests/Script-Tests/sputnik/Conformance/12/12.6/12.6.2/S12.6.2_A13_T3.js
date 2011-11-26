// Copyright 2009 the Sputnik authors.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/**
* @name: S12.6.2_A13_T3;
* @section: 12.6.2;
* @assertion: FunctionDeclaration within a "while" Statement is not allowed;
* @description: Checking if declaring a function within a "while" Statement that is in a function body leads to an exception;
* @negative;
*/

function(){

while(0){
    function __func(){};
};

};
