# Gameplay

Long-range, methodical, ~Splispace.

## Defense

Relies on evasion, point-defense, and managing your signature. 

Each ship has a scan range and a signature. Signature increases when firing certain weapons, and decreases each tick, down to a set minimum. Ship A can 
accurately target ship B with turrets and fire missiles at it if dist(A,B) <= scan(A) + signature(B). The idea is that holding fire is roughly analoguous to 
taking cover.

Point defense can shoot down missiles (maybe all projectiles?), and can engage ships as well. PD turrets will prioritize targets as follows:
* missiles targeting the ship based on angle and distance
* other ships/missiles based on angle and distance

Turrets have a limited rotation speed, and will attempt to predict the target's position. As of yet, do not account for angular velocity. All projectiles have
travel time, allowing the target to dodge them.

## Offense

## Player awareness
