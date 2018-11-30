# SBC
SamCo's Single Board Computer (65C02)

This single board computer was constructed for fun.  It has the
following specifications:

* 65C02 processor
* 32K RAM (with 256 addresses reserved for I/O)
* 32K ROM 
* 65C22 Versatile Interface Adapter (VIA)
* 65C51 Asynchronous Communications Interface Adapter (ACIA)
* 2x16 LCD
* Support for USB or external 5V power

After trying out writing programs in assembly and in BASIC, I found
the [Tali Forth 2](https://github.com/scotws/TaliForth2) project, and
that's what I've been using as an operating system/programming
language ever since.

![A 65C02-based Single Board Computer](https://github.com/SamCoVT/SBC/raw/master/SamCo_SBC.jpg)

## Memory Map

The memory map is currently as follows:

|Memory Range | Function|
|-------------|--------|
|$0000-$7EFF  | RAM|
|$7F00-$7F3F  | VIA (65C22 - parallel I/O)|
|$7F40-$7F7F  | EXSEL1 (External Select #1) (Active Low)|
|$7F80-$7FBF  | ACIA (65C51 - Serial port)|
|$7FC0-$7FFF  | LCD|

The EXSEL1 line is brought out to the expansion bus on the righ
side of the PCB and is available for use.  The EXSEL2 line is an
active-high version of the /RESET signal.  Both of these signals are
not currently used by the SBC and can be repurposed by reprogramming
the GAL.

The memory decoding is handled by a GAL22V10, programmed from the
ADDECODE.PLD file.  I used Wincupl (run under Wine on Linux) to
generate the .jed (JEDEC) file, and then I used Galblast on an old
Windows 98 machine with a parallel port adapter to program the GAL.

## Errata

* Because I used a WDC65C22S (which has a totem-pole IRQ output rather
  than the WDC65C22N which has an open drain IRQ output,) I had to cut
  the IRQ trace and add a Schottky diode (cathode towards pin 21 of
  IC6) across the gap.

* I needed to add approximately 30pF of capacitance to the ACIA
  crystal to get it to start up and oscillate reliably.  No provision
  was made on the board for this, so I added it on the back side of
  the board between pins 1 and 6.  I also needed to add a 1Meg Ohm
  resistor across the crystal pins when using an RCA CDP65C51 (but did
  not need this when using a Rockwell part.)


# License
The Eagle schematic board files are provided here under a Creative
Commons BY-SA 2.0 (CC BY-SA 2.0) license.
