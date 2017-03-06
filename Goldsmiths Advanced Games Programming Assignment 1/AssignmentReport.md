# Advanced Programming For Games - Assignment 1  
## Global Game Jam and Sumo Digital Rising Star  

--------------------------------------------------------------------------------

For this assignment we have been tasked with demonstrating a progression in skill level within the application of our programming and games production knowledge. I have chosen to divide my time between two external projects: the Global Game Jam and Sumo Digital Rising Star events.

N.B. Originally I signed up for the Search for a Star program, however, I was contacted by the organisers and told that, as I am a part time Masters student and thus will not be graduating until 2018, they would move me to the Rising Star contest track.

Each project will be covered separately within the following report.

--------------------------------------------------------------------------------

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/00_GameTitle.jpg "Before the Storm")  

## Global Game Jam 2017  
### January 19th - 22nd  

**Project repository**  
*github.com/MatthewDuddington/GGJ_2017*  
**Project video**  
*youtube.com/watch?v=Nblo7VLYo1U*

For this brief I lead the organisation of a team of six people, to undertake the 48h global game creation:

Matthew Duddington - Project manager (and supplementary programmer / designer)  
Witek Gawlowski - Technical Lead  
Robert Doig - Programmer  
Nikita Veselov - Programmer  
Pablo Larenas - Design Lead  
Jade Leamcharaskul - Musician  

The team was structured with a split hierarchy in order to facilitate the rapid decision making process required for short deadline game jams. The following is the summary which I set out for the team to understand this.

***Team Roles***

  Pablo will be 'Design Lead'  
  He will be responsible for the overall VISION of the game from an artistic and feel perspective.  
  Matthew be assisting him with additional art, animation etc as needed (when not fulfilling my other role - see below). Also, he will have assistance from Jade, with design, music and sound etc.

  Witek will be 'Technical Lead'  
  He will be responsible for determining the programmatic FEASIBILITY of ideas, features and mechanics proposed.  
  Robert and Nikita will work on different programming tasks, with the Technical Lead jumping back and forth as the partner in order to 'program in pairs'.

  Matthew will be 'Project Manager'  
  He will be responsible for maintaining TIME and SCOPE of the project, and supporting wellbeing and communication as needed.  
  He will have the final decision on issues where a consensus is not being easily reached (i.e. he is the top of the hierarchy) - though in 'normal' circumstances the Design and Technical leads will collaboratively define the direction.

### Practice Jam  

As this would be the first game jam for several members of the team and the first time several of us had worked together, we decided it would be prudent to undertake a miniature 6 hour practice jam the day before the real jam began. This proved to be highly beneficial as, while our overall design idea was strong, we discovered flaws in how ideas were naturally interpreted by different members of the team and how each individual’s working preference either worked in tandem with or in opposition to the others.

This enabled us to prepare a plan of approach for the GGJ itself the next day:  

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/01_GeneralPlan.jpg  "General plan")  

### Game Jam Day 1  

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/02_Welcome.jpg "Welcome presentation")  

For the GGJ itself, we reprised the same team structure but also armed with these adjustments to our approach. At 5pm the theme for this year was announced as being ‘Waves’. We used the first half hour of our time to each independently formulate some initial ideas, after coming together with a ‘no idea is bad’ methodology, we selected a few promising ideas and collaboratively discussed iterations until we settled on an idea developed from Robert Doig’s moving blocks concept.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/03_Ideas1.jpg "Ideas whiteboard 1") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/04_Ideas2.jpg "Ideas whiteboard 2")  

The basic starting premise was moving a player character around a surface of rising and falling cubes that could be manipulated in height by applying forces, the level’s surface might move with a sine wave like pattern and the goal would be to traverse the surface to a goal location. This tentatively had a water and shipwreck theme assigned to it. As project manager I led the construction, prioritisation and simplification of an first sprint plan, discussing with the team what the most important elements of the idea were to ‘prove’ first for us to have a working super basic prototype.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/05_Sprint1.jpg "Sprint 1 sheet")  

At this stage Pablo and I broke away to discuss gameplay development of the idea, while Witek organised his programming sub team based on the sprint plan - as the Goldsmiths’ event was to be moved to a new location within the first 2 hours, the programming was agreed to begin after the move, when we would be able to set up equipment properly. The gameplay development process evolved quickly into physical prototyping in the hallway, where the floor tiles provided a visual grid to work from. The idea evolved into one of two opposing players attempting to collect a set of objects scattered across floating platforms formed from a shipwreck. Several iterations in the physical prototyping revealed challenges in balance and resolving stalemate situations, however, these were resolved through a variety of changes to the design. Before the movement of the event, we had arrived at the conclusion of moving the players off of floating platforms and into lifeboats as well as introducing weapon pickups that would enable to players to affect the water’s surface in order to improve their position or disadvantage the other player.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/06_PhysicalPrototype.jpg "Physical prototyping") 

After relocating to the main jam location the programming team quickly assembled a working version of the water’s surface and the boat movement. From this we were already able to identify some points of delight and refinements to be made to the core systems. This became the basis for sprint 2, which I again lead the process of ratifying into a minimal and priorities set of tasks to be carried out.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/07_Sprint2.jpg "Sprint 2 sheet")  

While this process was underway, Pablo and I once again broke away to develop the gameplay further based on the experience the code prototype had created for the team. This process lead us away from a shipwreck and towards a pair of battling gallons, completing over resources in an open sea. Several ideas were formed but ultimately put aside as too complex or distracting from the core idea (such as a sea monster being the source of the waves at the centre of the map and something the players could interact with in order to change how the water on the map behaved; and the use of multiple different weapons to interact with the water around the other player).

Ultimately, we decided that the focus of the game should remain on the waves as much as possible and, thus, that the PvE element of play needed to be front and centre. This is where the constantly changing water surface that pushes you around the map was chosen as the key feature and the collectable gold was given a ‘purpose’ beyond simple greed and score - it became a means of survival by weighing down the ships making them more impervious to the waves’ forces.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/08_MainIdea.jpg "Main idea whiteboard") 

Because the nature of this idea was a non-traditional PvP style of play, we needed to sell the concept to the team. Through negotiations and some ‘final say’ estimations by me as project manager, we agreed on a format to take forwards. Once again a list of proposals for refinement were identified for the next mid sprint session.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/09_Proposals.jpg "Proposals 1 sheet") 

After some initial work setting up the structure for these behaviours the team agreed to break for the night and pick-up on the main bulk of the next stage of the prototype’s development in the morning.

### Game Jam Day 2  

At this stage the design was sufficiently well defined and with a list of both code and art assets that I was able to switch to my programming role under Witek’s management, while Pablo focused on preparing some early art assets. Here I created functionality for spawning and collecting of coins, and the ‘iron clad’ system of pickups that would provide a way for players to collide with each other in an asymmetrical way to force the dropping of coins.

Through the day, in between project management duties, I also added a physics based dispersal system for the coins, setup the coins and pickups to work on an object pool basis (to avoid repeated instantiation and destruction of objects within memory) and a day night cycle structure for driving the timing of the wave changes. The most important part of my role however, was fielding communication between team members, keeping track of progress / the direction the project was heading and, crucially, defining high level tasks for each group. By midday we had defined a list of balancing mechanics that would need to be play tested in the upcoming prototype.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/10_PlaytestList.jpg "Playtest list") 

Initially, we had hoped to be able to test this by 6pm that evening, however, difficulties in resolving the wave system with transitions as well as sharing interdependent code that was being simultaneously refactored caused a number of lengthly delays. By late that evening we were finally able to test the refined gameplay - to our delight, the game was fun to play and intuitively suggested points for improvement and development.

Infused with energy of experiencing the prototype and nervous of the overrun in time we had needed to spend to reach this point, many of the team members wished to work through the second night on the next series of changes. With the slightly reduced team, many aspects of the wave system were restructured and cleaned up by Witek and Robert, before they were once again joined by Nikita the following morning who, with a rested mind, was able to pick up the programming relay.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/11_NightTeam.jpg "Night team") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/12_ArtDIrectorList.jpg "Art director list") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/13_TaskTickSheet.jpg "Task tick sheet") 

Throughout this, Pablo was working on polishing the artwork side and Robert developed the user interface with him. I assisted with small sections of code debugging and art production as well as keeping track of how tasks should progress to help combat the team’s minds succumbing to fatigue. Unfortunately, during the next morning, Pablo was taken ill from the exhaustion of the previous nights work and I had to break away from the project to help him recover and assist with getting him sent back home to rest. However, he left us with the majority of the art work finished that I was able to close off the remaining elements.

With a final push towards the afternoon of the final day, we were able to complete work on the main body of gameplay, such that the prototype demonstrated the game in an entertaining and playable form. With some last minute input from Jade, who had been working with me on the sound and music during the final day, and some break neck squashing of bugs, the final production was quite well rounded.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/14_RobProgramming.jpg "Robert programming") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/15_Gameplay.jpg "Before the Storm gameplay") 

The production was well received by the audience at the end-of-event show and tell and the team has felt moved to continue to develop the title with a view to releasing it on Steam Greenlight sometime in the coming months. Witek is currently hard at work on this process!

--------------------------------------------------------------------------------

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/16_UClear.jpg "Uclear boffins close up") 

## Search for a Star / Sumo Digital Rising Star 2017  

**Project repository**  
*github.com/MatthewDuddington/UClear*  
**Playable Game Link & Itch.io Gamepage**  
*https://owlkinmd.itch.io/uclear*  

As was mentioned at the start of my report I was placed onto the ‘Rising Star’ track rather than ‘Search for a Star’; to all intents and purposes these appear to have been identical coding tasks and game jam starting code / briefing, however, my impression is that the expectations for level of achievements and delivery is scaled differently for each group. This likely, favoured my progress into stage 2 following the code test. At the time of writing, the results and feedback for stage 2 have not yet been made known.

### Stage 1 - Code Test  

Stage one involved undertaking a timed (90 minute) set of programming challenge questions within the Hacker Rank web platform. There were 6 questions, of which I was able to answer 3 correctly within the time, while also beginning a forth; there was also a practice question beforehand which I was able to answer correctly.

**Practice Question**

The practice question provided an input integer and required the completion of a function StairCase() that would print via STDOUT a visual representation of a staircase of the order of the given integer that used # symbols as building blocks, e.g.  
  
        #  
       ##  
      ###  
     ####  
    #####  
  
My answer was as follows:

    Void StairCase(int n) {
        for (int i = 1; i <= n; i++) {
            for (int j = 0; j < n - i; j++) {
                cout << " ";
            }
            for (int k = 0; k < i; k++) {
                cout << "#";
            }
            cout << "\n";
        }
    }

**Answered Questions**

Following are screen shots of the three questions I was able to answer in the time available and my code snippets.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/17_Q1.jpg "Question 1") 

    #include <iostream>
    #include <string>

    using namespace std;

    int main()
    {
        /*
        Enter your code here. Read input from STDIN. Print output to STDOUT.
        You may also change the code outside of main, such as to add new #includes
        */

        std::string input;
        std::getline(std::cin, input);

        int sum = 0;
        for (int i = 0; i < input.size(); i++) {
            if (isupper(input[i])) {
            sum++;
            }
        }

        std::cout << sum;

        return 0;
    }

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/18_Q3.jpg "Question 3") 

    #include <iostream>
    #include <string>

    using namespace std;

    int main()
    {
        /*
        Enter your code here. Read input from STDIN. Print output to STDOUT.
        You may also change the code outside of main, such as to add new #includes
        */

        std::string input;

        while (std::getline(std::cin, input)) {

            for (int i = 0; i < input.size(); i++) {
            char c = input[i];
            if (c == ' ') {
                std::cout << ' ';
            }
            else if (c == '\t') {
                std::cout << '\t';
            }
            else {
                std::cout << input[i] << input[i];
            }
            }
            std::cout << '\n';

        }

        return 0;
    }

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/19_Q4.jpg "Question 4") 

    #include <iostream>
    #include <vector>

    using namespace std;

    int main()
    {
        /*
        Enter your code here. Read input from STDIN. Print output to STDOUT.
        You may also change the code outside of main, such as to add new #includes
        */

        int cols;
        int rows;
        std::cin >> cols >> rows;

        std::vector< vector<int> > matrix;
        matrix.resize(rows);

        int input = 0;
        for (int r = 0; r < rows; r++) {
            matrix[r].resize(cols);
            for (int c = 0; c < cols; c++) {
            std::cin >> input;
            matrix[r][c] = input;
            }
        }

        for (int c = 0; c < cols; c++) {
            for (int r = 0; r < rows; r++) {
            std::cout << matrix[r][c] << ' ';
            }
            std::cout << '\n';
        }

        return 0;
    }

**Record of Remaining Questions**

For interest, these were the remaining three questions in the test. I had began to answer the reverse linked list question when time ended for the test.  

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/20_Q2.jpg "Question 2") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/21_Q5.jpg "Question 5") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/22_Q6.jpg "Question 6") 

I felt that, with more time, I would have been able to answer the other questions in the test. Though in all cases I required time to look up the standard guidelines in order to know which features to use.  

--------------------------------------------------------------------------------

### Stage 2 - Game Jam  

For the second stage of the competition we were provided with a Unity project with some starter code and a very open set of guidelines. i.e. that we must create and upload a game that built upon the starter code in an interesting way. The code we were provided with consisted of loading, UI and camera mangers as well as behavioural code that created a pair of rudimentary flocking behaviours.

After spending some time interpreting the starter code, the design I devised from seeing these behaviours play out was a top down, agility puzzle game who’s core mechanic would be based on a realtime implementation of the classic board game Labyrinth’s tile rearrangement system. However, rather than have players take turns to collect items, my game would require a player to rearrange the board on the fly to funnel / guide a series of computer AI controlled agents to a destination tile.

The starter code had a behaviour whereby certain objects objects would be magnetised to the player, which inspired the idea that the AI agents would be trying to seek the player and that this behaviour would be a part of how the player would guide them through the map - they would need to risk being touched by one of the agents in order to direct their movement. I also wanted to try and have groups of agents wandering the map together in mini packs, which would make use of the flocking starter code.

I began by implementing the tile system I would require for the map grid. Using the Labyrinth board game as a requirements template, I built rudimentary white box assets within Unity and planned out a data structure that would enable me to do three things:
* Setup certain pre-fixed tiles that all maps should have
* Randomly distribute tiles amongst the remaining spaces
* Be adaptable to the position of each tile, so that they could be rearranged during play

Within a Map class I used a two dimensional array to store a series of references to Tile class instances. Each tile would handle its own movement during the column or row sliding behaviours, which would in-turn be controlled by the outer map class. Utilising array count mathematics enabled me to create many of these behaviours as nested loops.

While I was creating the working prototype of this system I realised that it would be beneficial and more challenging overall to make the map size adaptable. Therefore, I ensured that all my array calculations referenced Min/Max type values. Though, as the classic map design assumed an odd numbered size of map on each axis, I also ran a check that would adapt the user values if necessary.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/23_Map.jpg "Normal sized map") 
![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/24_MapSizes.jpg "Example size maps") 

With the map structure working relatively quickly, I started work on the AI system. However, this lead to the largest problem with my project. While I understood the principle of how the current steering behaviours worked, what I wanted from my AI and what kinds of behaviours would achieve this, when all the different behaviour parts interacted with each other, they did not have the desired effect, and I repeatedly found myself struggling to achieve movement that didn’t simply cancel out each opposing vector or alternatively moved the agents but which would quickly drive them into a wall and get stuck there.

Despite quite a number of iterations, some of which can be seen within the commits, I found myself rapidly running short of time before the deadline and so eventually I decided it was most important to be able to submit a game that was playable rather than non-functional code that stuck more rigidly to the brief.

This turned out to be a wise decision, as within a short space of time I was able to achieve an AI behaviour that replicated the kind of actions I had originally intended, even though it was less emergent than I had hoped. This required adding a kind of intelligence to each tile so that agents could request information about what exits were available to them and then make decisions about which tile to move to next. In combination with some stochastic behaviour, this created a pleasing type of movement from the agents.

With the assistance of Pablo Larenas creating some art assets based on my briefing, I was able to apply a cartoony visual style that fit the comedic, radiation spill scenario I had devised to explain the games’ task and player abilities.

In the shipping version of the game there are a number of inefficiencies. Primarily, it uses a great idea of ray casting to perform various checks. Many of these could be handled much better by explaining on the use of the intelligent tiles with recursive checks on the status of adjacent tiles based on some logical query. For example, each agent ‘leader’, during their AI tick, checks its ability to see (or not see) the player from their location with a ray cast. This would be better handled by having the players tile check each connected adjacent tile with valid matching exit borders for the presence of a leader agent; if any are found then that agent would handle the hunting behaviour. This would only need to be resolved once per tick rather than through multiple recasts. Similarly every time a tile set is slid along a row or column the whole map re-checks which exits are now valid. This could be made more efficient by only asking tiles adjacent to the changes row / column to perform that check.

While this segment of the project did not require me to make use of C++, the challenge of implementing the rearrangeable tile grid was an interesting one, and one which I can see being very useful to my work in future games. Likewise, the challenge of devising and implementing a game on my own, in what felt to be a relatively short space of time, was both scary and motivating. Though I was unable to spend very much time polishing the gameplay because of the AI struggles consuming much of my time, I am pleased with the result as a starting point for a project I would like to continue developing later in the year.

![alt text](https://github.com/MatthewDuddington/UClear/blob/master/Goldsmiths%20Advanced%20Games%20Programming%20Assignment%201/Report%20Images/25_UclearGameplay.jpg "UClear gameplay") 
