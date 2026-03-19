// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/02/Inc16.tst

load Inc16.hdl,
output-file Inc16.out,
compare-to Inc16.cmp,
output-list in%B1.16.1 out%B1.16.1;

set in %B0000000000000000,  // in = 0
eval,
output;

set in %B0000000000000001,  // in = 1
eval,
output;

set in %B1111111111111111,  // in = -1
eval,
output;

set in %B0000000000000101,  // in = 5
eval,
output;

set in %B1111111111111011,  // in = -5
eval,
output;

set in %B0000000011111111,  // in = 255
eval,
output;

set in %B0000000111111111,  // in = 511
eval,
output;

set in %B0111111111111111,  // in = 32767
eval,
output;

set in %B1111101111111111,  // in = -1025
eval,
output;

set in %B1111101111111011,  // in = -1029
eval,
output;
