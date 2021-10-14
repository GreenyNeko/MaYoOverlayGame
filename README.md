# MaYoOverlayGame
Osu inspired Kanji learning overlay game

## How To Use

### General

The way I intended the program to be used (doesn't mean you have to use it like that ;))
Read some Japanese and hit the respective key depending on how you faired.
If you are able to read the Kanji correctly press the key for correct hit.
If you got a correct reading for the Kanji but it's not used in this case (e.g. 人 is hito but in 人生 it is jin)
then press the key for sloppy.
If you don't know how the kanji is read or you used a wrong reading it counts as a miss.

### Compound Words

If I don't know a reading of at least one Kanji in a compound word I may not be able to look up the correct reading.
In this case I count all other Kanji in the compound word that I would know as sloppy.

## Controls

Currently the keys are hard coded.
When you press *1* on the numpad the overlay will count that as a correct hit.
Pressing *2* on the numpad will be counted as a sloppy hit,
and pressing *3* on the numpad will count as a miss.
When you press *Esc* at any point it will show the end result screen.
You can leave the end result screen by pressing *Esc* again, which will reset the progress and scores.

## To-Do

Version 1.0:
* Code documentation
* Decouple the low level key hook off of the program to ensure that it doesn't cause windows to shut the program down
* Implement a configuration for the keys