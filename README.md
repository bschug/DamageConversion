# Legendary Play Gameplay Developer Test

This test is meant to evaluate your ability to understand and implement complex game design requirements.
It should not take more than a day to complete it.

Imagine you have just been hired as a gameplay developer at Grinding Gear Games. Unfortunately, on your
very first day, you managed to delete the code for the damage conversion system of Path of Exile, and all
of the backups.
You have to quickly rebuild it before anyone notices. Fortunately, the wiki documents the system in great
detail, and you did not delete the unit tests.
You will also need to pretend that Path of Exile is written in C#, because this metaphor only goes so far.


You can find the documentation for how the system should work on the wiki:
https://www.poewiki.net/wiki/Damage_conversion

You can ignore the parts about sources (sections 2+3), focus on the behaviour and the example calculations
(sections 1+4).

In the Solution, you find the tdd-poe.csproj, which defines some data types and interfaces.
It also contains the CharacterDamage class, which has a CalculateDamage method that you need to implement.

In the DamageConversionTests.csproj, you will find a set of tests that correspond to the examples from the wiki.
Your solution needs to pass all of these tests.

You may change the contents of DamageConversion.csproj as you see fit. You may also add new tests if you like.
But you must not make any changes to the existing tests. That includes that you may not make breaking changes
to the interfaces and data structures the tests are using. You may extend them, as long as this does not require
changes to the tests.

Good luck, and have fun!
