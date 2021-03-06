# Multi Agent project  
### Main idea
Path planning for cars to kill all enemies as long as avoid obstacles in a given maze. There are 5 problems with different settings. Each problem is setup in a different "Scene" of the unity project. The AI strategies are under `MultiAgent project/Assets/Scrips/`.

#### Problem 1: Short range multi agent search
Setting: the search area is limited within a short range.

<img src="https://drive.google.com/file/d/1cnctqvZVHbFxToO03dk2cn3j-ge4LNo3/view?usp=sharing" alt="problem1">

The left window: from a global camera. The long white line indicates the car and the turret is able to see and attack each other.  
The right window: from a car camera. The green bar above the car shows its "blood". The number of enemies left to kill is shown on the top of the window.

#### Problem 2: Long range multi agent search  
Setting: the search area is limited only by obstacles.

<img src="https://drive.google.com/file/d/13GHccKh5PN7kkq0czJWPWlzzlTxwc_sp/view?usp=sharing" alt="problem2">

The red and blue lines indicate the sight blocked by obstacles. When they turn to white, cars and turrets are able to detect and attack each other.

#### Problem 3:  Vehicle Routing Problem (VRP)  
Setting: given location of all enemies, solve VRP to find an overall optimal path organization for all cars.

<img src="http://drive.google.com/uc?export=view&id=1eO88yhNXUfxAr0prFq8plIFfBi4uDjgs" alt="problem3">

#### Problem 4: Formation sweep  
Setting: given a leader car, the other cars need to follow it and march in formation.

<img src="https://drive.google.com/file/d/1wEecDqrOKSH1Ep9xrD6l7XvW1RqN1R4H/view?usp=sharing" alt="problem4">

#### Problem 5: Shooter coordination  
Setting: the car is easy to "die". Thus they need to move in coordination with other agents (behind obstacles)while battling against enemies. 

<img src="https://drive.google.com/file/d/1ppChGPLSOaRjipLGgVXwd1aTjWsaoqjR/view?usp=sharing" alt="problem5(1)">
<img src="https://drive.google.com/file/d/1Js0OUkFO6OZ_msYTcWxMloTX_EL81Gl6/view?usp=sharing" alt="problem5(2)">

Circles in the same color show the locations where the left cars need to arrive at the same time. 
