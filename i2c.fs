\ Set up the VIA for I2C.  I'm using the VIA DDR method.

\ PTA7 is data  
\ PTA0 is clock 

hex
7F01 constant via.porta
7F03 constant via.ddra
\ Make port A an input so the bus starts idle.
: i2c-setup 0 via.porta c! 0 via.ddra c! ;

\ Data on PORTA7 (note that 0 = 1 on the I2C bus for writing)
\ : i2c-sda0 via.ddra c@ 80  or via.ddra c! ;  allow-native
\ : i2c-sda1 via.ddra c@ 7f and via.ddra c! ;  allow-native

: i2c-sda0
    [
    AD c, 03 c, 7f c, ( lda $7f03 )
    09 c, 80 c,       ( ora #$80  )
    8D c, 03 c, 7f c, ( sta $7f03 )
    ] ; allow-native

: i2c-sda1
    [
    AD c, 03 c, 7f c, ( lda $7f03 )
    29 c, 7F c,       ( and #$7F  )
    8D c, 03 c, 7f c, ( sta $7f03 )
    ] ; allow-native
    

\ Clock is on PORTA0 (note that 0 = 1 on I2C bus)
\ : i2c-scl0 via.ddra c@ 01  or via.ddra c! ;  allow-native
\ : i2c-scl1 via.ddra c@ FE and via.ddra c! ;  allow-native

: i2c-scl0
    [
    AD c, 03 c, 7f c, ( lda $7f03 )
    09 c, 01 c,       ( ora #$01  )
    8D c, 03 c, 7f c, ( sta $7f03 )
    ] ; allow-native
: i2c-scl1
    [
    AD c, 03 c, 7f c, ( lda $7f03 )
    29 c, FE c,       ( and #$FE  )
    8D c, 03 c, 7f c, ( sta $7f03 )
    ] ; allow-native

\ Clock the bus high, then low.
: i2c-clock
    i2c-scl1 i2c-scl0 ;  allow-native

\ Generate a START condition on the bus.
: i2c-start
    i2c-sda1 i2c-scl1 i2c-sda0 i2c-scl0 ;  allow-native

\ Generate a STOP condition on the bus.
: i2c-stop
    i2c-sda0 i2c-scl1 i2c-sda1 ;  allow-native

\ Transmit a single bit.
: i2c-txbit ( bit -- )
    if i2c-sda1 else i2c-sda0 then i2c-clock ;

\ Receive a single bit.
: i2c-rxbit ( -- bit )
    i2c-sda1 i2c-scl1 via.porta c@
    80 and if 1 else 0 then i2c-scl0 ;

: i2c-tx ( byte -- nak )
    8 0 do dup 80 and i2c-txbit 2* loop drop ( Send the byte )
    i2c-rxbit ; ( Get the NAK flag )

: i2c-rx ( nak -- byte )
    0 8 0 do 2* i2c-rxbit + loop ( Receive the byte )
    swap i2c-txbit ; ( Send the NAK flag )
    
: block2eeprom ( u -- u u ) ( blocknum -- eeprom_address i2c_address )
    ( Note that the i2c_address returned has already been shifted to )
    ( the left by one bit.  The R/*W bit in bit 0 starts as a zero.  )
    dup 40 < if
        ( Blocks 0-63[decimal] )
        400 * ( multiply block number by 1024[decimal] )
        A0    ( use $50 [shifted left one place] as I2C address )
    else
        ( Blocks 64-127[decimal] - no limit check )
        40 -  ( subtract 64[decimal] from block number )
        400 * ( multiply block number by 1024[decimal] )
        A8    ( use $54 [shifted left one place] as I2C address )
    then ;

: eeprom-pagewrite ( addr u u -- ) ( buffer_address eeprom_address i2c_address -- )
    dup >r ( save the i2c address for later )
    i2c-start i2c-tx drop ( start the i2c frame using computed i2c address )
    100 /mod i2c-tx drop i2c-tx drop ( send the 16-bit address as two bytes )
    80 0 do ( send the 128[decimal] bytes )
        dup i +     ( compute buffer address )
        c@ i2c-tx drop ( send the byte )
    loop drop i2c-stop ( end the frame )
    r> begin ( recall the i2c address and poll until complete )
        dup
        i2c-start i2c-tx ( start the i2c frame using computed i2c address )
    0= until drop
    i2c-stop
    ;


: eeprom-blockwrite ( addr u -- ) ( buffer_address blocknum -- )
    ( Write the entire block buffer one eeprom page [128 bytes] at a time )
    8 0 do
        over i 80 * +      ( offset by eeprom pages into block buffer )
        over block2eeprom
        swap i 80 * + swap ( offset by eeprom pages into eeprom )
        eeprom-pagewrite
    loop
    2drop ;

: eeprom-blockread ( addr u -- ) ( buffer_address blocknum -- )
    block2eeprom dup
    i2c-start i2c-tx drop ( start the i2c frame using computed i2c address )
    swap ( move the eeprom internal address to TOS )
    100 /mod i2c-tx drop i2c-tx drop ( send the 16-bit address as two bytes )
    i2c-start 1+ i2c-tx drop ( send I2C address again with R/W* bit set )
    3FF 0 do ( loop though all but the last byte )
        0 i2c-rx over i + c! 
    loop
    ( Read last byte with NAK to stop )
    1 i2c-rx over 3FF + c! i2c-stop drop ;

\ Connect to Tali Forth 2 BLOCK words
' eeprom-blockread  is BLOCK-READ
' eeprom-blockwrite is BLOCK-WRITE

decimal
\ =======================================================    

    

