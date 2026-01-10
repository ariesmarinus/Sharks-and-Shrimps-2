using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class grid_manager : MonoBehaviour
{
    public moving_script cell;
    public enum Animal
    {
        Shark,
        Fish,
        Shrimp
    }
    public List<Animal> white_animals = new List<Animal>{ Animal.Shark, Animal.Fish, Animal.Shrimp };
    public int current_animal_white = 0;
    public List<Animal> green_animals = new List<Animal>{ Animal.Shark, Animal.Fish, Animal.Shrimp };
    public int current_animal_green = 0;

    public List<AnimalData> escapable_animals = new List<AnimalData>{};
    public List<Animal> escaped_white = new List<Animal>{};
    public List<Animal> escaped_green = new List<Animal>{};



    public enum GridSquareAnimal
    {
        EMPTY,
        WHITE_SHARK,
        WHITE_FISH,
        WHITE_SHRIMP,
        GREEN_SHARK,
        GREEN_FISH,
        GREEN_SHRIMP,
    }

    public enum GridSquareBubbles
    {
        EMPTY,
        BUBBLES
    }

    // Accessed x,y board[x][y]
    public GridSquareAnimal[][] animal_board;
    public GridSquareBubbles[][] green_bubble_board;
    public GridSquareBubbles[][] white_bubble_board;

    // 
    // |----------------
    // | WS |     |      ...
    // |----------------
    // |    |  GS |
    // |----------------
    // |
    //

    public enum Team
    {
        White,
        Green
    }

    [Serializable]
    public class AnimalData
    {
        public GameObject game_object;
        public Vector2Int grid;
        public GridSquareAnimal type;
        public Team team;
        public Animal animal;
    }
    public Team current_team = Team.White;

    public AnimalData white_shark      = new AnimalData();
    public AnimalData white_fish       = new AnimalData();
    public AnimalData white_shrimp     = new AnimalData();
    public AnimalData green_shark      = new AnimalData();
    public AnimalData green_fish       = new AnimalData();
    public AnimalData green_shrimp     = new AnimalData();
    
    public GameObject white_bubbles;
    public GameObject green_bubbles;
    public GameObject white_wins;
    public GameObject green_wins;
    public GameObject draw;
    public GameObject escape_white_button;
    public GameObject escape_green_button;
    public GameObject menu;


    void ChangeTeam()
    {
        if (current_team == Team.White)
        {
            if (green_animals.Count > 0)
            {
                current_team = Team.Green;
            }
            else
            {
                if (white_animals.Count > 0)
                {
                    current_team = Team.White;  
                }
                else
                {
                    DefineWinner();
                }
            }
        }
        else if (current_team == Team.Green)
        {
            if ( white_animals.Count > 0)
            {
            current_team = Team.White;
            }
            else
            {
                if (green_animals.Count > 0)
                {
                    current_team = Team.Green;  
                }
                else
                {
                    DefineWinner();
                }
            }
        }
        EscapeButtons();
    }

    void DefineWinner()
    {
        if ( escaped_white.Count > escaped_green.Count)
        {
            white_wins.SetActive(true);
        }
        else if (escaped_green.Count > escaped_white.Count)
        {
            green_wins.SetActive(true);
        }
        else if (escaped_white.Count == escaped_green.Count)
        {
            draw.SetActive(true);
        }
        BubblesGoUp();
    }

    int ResetCurrentAnimalIndex(int current_animal_index, int number_animals)
    {
        if ( number_animals > 0 )
        {
            return current_animal_index % number_animals; 
        }

        return 0;
    }

    int NextAnimal( int current_animal_index, int number_animals )
    {
        return (current_animal_index + 1) % number_animals;
    }

    void ChangeAnimalGreen()
    {
        current_animal_green = NextAnimal(current_animal_green, green_animals.Count);
    }

    void ChangeAnimalWhite()
    {
        current_animal_white = NextAnimal(current_animal_white, white_animals.Count);
    }

    Animal CurrentAnimalGreen()
    {
        return green_animals[current_animal_green];
    }

    Animal CurrentAnimalWhite()
    {
        return white_animals[current_animal_white];
    }

    AnimalData GetAnimalTurn(Team team)
    {
        if (team == Team.White)
        {
            if ( white_animals.Count == 0 )
            {
                return null;
            }

            Animal animal = CurrentAnimalWhite();
            switch (animal)
            {
            case Animal.Shark:
                return white_shark;
            case Animal.Fish:
                return white_fish;
            case Animal.Shrimp:
                return white_shrimp;
            }
        }
        else
        {
            if ( green_animals.Count == 0 )
            {
                return null;
            }

            Animal animal = CurrentAnimalGreen();
            switch (animal)
            {
            case Animal.Shark:
                return green_shark;
            case Animal.Fish:
                return green_fish;
            case Animal.Shrimp:
                return green_shrimp;
            }
        }

        Debug.Assert(false, "No animal turn");

        return null;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 64; i ++)
        {
            moving_script obj = Instantiate(cell, transform, false);
            obj.grid_manager = this;
            obj.x = i % 8;
            obj.y = i / 8;
        }

        animal_board = new GridSquareAnimal[8][];
        green_bubble_board = new GridSquareBubbles[8][];
        white_bubble_board = new GridSquareBubbles[8][];
        for ( int x = 0; x < 8; x++ )
        {
            animal_board[x] = new GridSquareAnimal[8];
            green_bubble_board[x] = new GridSquareBubbles[8];
            white_bubble_board[x] = new GridSquareBubbles[8];
            for ( int y = 0; y < 8; y++ )
            {
                animal_board[x][y] = GridSquareAnimal.EMPTY;
                green_bubble_board[x][y] = GridSquareBubbles.EMPTY;
                white_bubble_board[x][y] = GridSquareBubbles.EMPTY;
            }
        }
        
        //Unity.Mathematics.Random random = new Unity.Mathematics.Random();
        //int random_x = random.NextInt(1, 2);
        //int random_y = random.NextInt(1, 3);

        SetAnimal(0, 1, Team.White, Animal.Shark);
        SetAnimal(0, 3, Team.White, Animal.Fish);
        SetAnimal(0, 5, Team.White, Animal.Shrimp);
        SetAnimal(7, 1, Team.Green, Animal.Shark);
        SetAnimal(7, 3, Team.Green, Animal.Fish);
        SetAnimal(7, 5, Team.Green, Animal.Shrimp);


        
    }

    GridSquareAnimal GetAnimalSquareType( Team team, Animal animal )
    {
        if (team == Team.White)
        {
            switch (animal)
            {
            case Animal.Shark:
                return GridSquareAnimal.WHITE_SHARK;
            case Animal.Fish:
                return GridSquareAnimal.WHITE_FISH;
            case Animal.Shrimp:
                return GridSquareAnimal.WHITE_SHRIMP;
            }
        }
        else
        {
            
            switch (animal)
            {
            case Animal.Shark:
                return GridSquareAnimal.GREEN_SHARK;
            case Animal.Fish:
                return GridSquareAnimal.GREEN_FISH;
            case Animal.Shrimp:
                return GridSquareAnimal.GREEN_SHRIMP;
            }
        }
        return GridSquareAnimal.EMPTY;
    }

    AnimalData GetAnimal( Team team, Animal animal )
    {
        if (team == Team.White)
        {
            switch (animal)
            {
            case Animal.Shark:
                return white_shark;
            case Animal.Fish:
                return white_fish;
            case Animal.Shrimp:
                return white_shrimp;
            }
        }
        else
        {
            
            switch (animal)
            {
            case Animal.Shark:
                return green_shark;
            case Animal.Fish:
                return green_fish;
            case Animal.Shrimp:
                return green_shrimp;
            }
        }

        Debug.Assert(false, "No animal");
        return null;
    }

    void MoveAnimal(AnimalData animal, int x, int y)
    {
        animal_board[animal.grid.x][animal.grid.y] = GridSquareAnimal.EMPTY;
        animal.grid = new Vector2Int(x,y);
        animal_board[x][y] = animal.type;
        animal.game_object.transform.position = GridToPosition(x,y);
    }

    void SetAnimal(int x, int y, Team team, Animal animal)
    {
        AnimalData animal_object = GetAnimal(team, animal);
        if ( animal_object != null )
        {
            animal_object.game_object.transform.position = GridToPosition(x,y);
        }
        GridSquareAnimal grid_square_animal = GetAnimalSquareType(team, animal);
        animal_object.type = grid_square_animal;
        animal_board[x][y] = grid_square_animal;

        animal_object.team = team;
        animal_object.grid = new Vector2Int(x, y);
        animal_object.animal = animal;
    }

    public List<GameObject> SpawnedBubbles = new List<GameObject>{};
    void SpawnBubbles(Team team, int x, int y)
    {
        if ( team == Team.Green )
        {
            green_bubble_board[x][y] = GridSquareBubbles.BUBBLES;
            GameObject bubbles = Instantiate(green_bubbles);
            SpawnedBubbles.Add(bubbles);
            bubbles.transform.position = GridToPosition(x,y);
        }
        else
        {
            white_bubble_board[x][y] = GridSquareBubbles.BUBBLES;
            GameObject bubbles = Instantiate(white_bubbles);
            SpawnedBubbles.Add(bubbles);
            bubbles.transform.position = GridToPosition(x,y);
            
        }
    }

    void KillAnimal(AnimalData animal)
    {
        if ( animal.team == Team.White )
        {
            EatAnimal(animal, white_animals);
            current_animal_white = ResetCurrentAnimalIndex(current_animal_white, white_animals.Count);
            switch (animal.animal)
            {
                case Animal.Shark:
                escapable_animals.Add(green_shrimp);
                break;
                case Animal.Fish:
                escapable_animals.Add(green_shark);
                break;
                case Animal.Shrimp:
                escapable_animals.Add(green_fish);
                break;
            }
            if (white_animals.Count < 1)
            {
                ChangeTeam();
            }
        }
        else
        {
            EatAnimal(animal, green_animals);
            current_animal_green = ResetCurrentAnimalIndex(current_animal_green, green_animals.Count);
            switch (animal.animal)
            {
                case Animal.Shark:
                escapable_animals.Add(white_shrimp);
                break;
                case Animal.Fish:
                escapable_animals.Add(white_shark);
                break;
                case Animal.Shrimp:
                escapable_animals.Add(white_fish);
                break;
            }
            if (green_animals.Count < 1)
            {
                ChangeTeam();
            }
        }
    }

    void EatAnimal(AnimalData eaten_animal, List<Animal> animals )
    {
        Debug.Log("is eaten");
        SpawnBubbles(eaten_animal.team, eaten_animal.grid.x, eaten_animal.grid.y);
        Destroy(eaten_animal.game_object);
        animal_board[eaten_animal.grid.x][eaten_animal.grid.y] = GridSquareAnimal.EMPTY;
        animals.Remove(eaten_animal.animal);
    }

    void EatAnimal(GridSquareAnimal current_animal_type, GridSquareAnimal destination_square_type)
    {
        switch ( destination_square_type )
        {
        case GridSquareAnimal.EMPTY:
            return;
        case GridSquareAnimal.WHITE_SHARK:
            if( current_animal_type == GridSquareAnimal.GREEN_SHRIMP)
            {
                EatAnimal(white_shark, white_animals);
                escapable_animals.Add(green_shrimp);
                current_animal_white = ResetCurrentAnimalIndex(current_animal_white, white_animals.Count);
                Debug.Log(current_animal_type + "can now escape");
                return;
            }
            else
            {
            return;
            }
        case GridSquareAnimal.WHITE_FISH:
            if (current_animal_type == GridSquareAnimal.GREEN_SHARK)
            {
                EatAnimal(white_fish, white_animals);
                escapable_animals.Add(green_shark);
                current_animal_white = ResetCurrentAnimalIndex(current_animal_white, white_animals.Count);
                Debug.Log(current_animal_type + "can now escape");
                return;
            }
            else
            {
                return;
            }
        case GridSquareAnimal.WHITE_SHRIMP:
            if( current_animal_type == GridSquareAnimal.GREEN_FISH)
            {
                EatAnimal(white_shrimp, white_animals);
                escapable_animals.Add(green_fish);
                current_animal_white = ResetCurrentAnimalIndex(current_animal_white, white_animals.Count);
                Debug.Log(current_animal_type + "can now escape");
                return;
            }
            else
            {
                return;
            }
        case GridSquareAnimal.GREEN_SHARK:
            if (current_animal_type == GridSquareAnimal.WHITE_SHRIMP)
            {
                EatAnimal(green_shark, green_animals);
                escapable_animals.Add(white_shrimp);
                current_animal_green = ResetCurrentAnimalIndex(current_animal_green, green_animals.Count);
                Debug.Log(current_animal_type + "can now escape");
                return;
            }
            else
            {
                return;
            }
        case GridSquareAnimal.GREEN_FISH:
            if (current_animal_type == GridSquareAnimal.WHITE_SHARK)
            {
                EatAnimal(green_fish, green_animals);
                escapable_animals.Add(white_shark);
                current_animal_green = ResetCurrentAnimalIndex(current_animal_green, green_animals.Count);
                Debug.Log(current_animal_type + "can now escape");
                return;
            }
            else
            {
                return;
            }
        case GridSquareAnimal.GREEN_SHRIMP:
            if (current_animal_type == GridSquareAnimal.WHITE_FISH)
            {
                EatAnimal(green_shrimp, green_animals);
                escapable_animals.Add(white_fish);
                current_animal_green = ResetCurrentAnimalIndex(current_animal_green, green_animals.Count);
                Debug.Log(current_animal_type + "can now escape");
                return;
            }
            else
            {
                return;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    static Vector2 GridToPosition(int x, int y)
    {
        return new Vector2(x - 4 + 0.5f, 4 - y - 0.5f);    
    }

    bool Trapped(AnimalData current_animal)
    {
        int x = current_animal.grid.x;
        int y = current_animal.grid.y;

        return 
            ! CanMoveToSquare(x - 1, y + 1, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x + 0, y + 1, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x + 1, y + 1, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x + 1, y + 0, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x + 1, y - 1, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x + 0, y - 1, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x - 1, y - 1, current_animal, green_bubble_board, white_bubble_board, animal_board) &&
            ! CanMoveToSquare(x - 1, y + 0, current_animal, green_bubble_board, white_bubble_board, animal_board);
    }

    void EscapeButtons()
    {
        AnimalData current_animal = GetAnimalTurn(current_team);

        if (escapable_animals.Contains(current_animal) && current_team == Team.White)
            {
                if (current_animal.grid.x+1 > 7 || current_animal.grid.x-1 < 0 || current_animal.grid.y+1 > 7 || current_animal.grid.y -1 <0)
                {
                    escape_white_button.SetActive(true);
                }
            }
        else if (escapable_animals.Contains(current_animal) && current_team == Team.Green)
            {
                if (current_animal.grid.x+1 > 7 || current_animal.grid.x-1 < 0 || current_animal.grid.y+1 > 7 || current_animal.grid.y -1 <0)
                {
                    escape_green_button.SetActive(true);
                }
            }
        else
        {
            if (escape_white_button.activeSelf == true)
            {
                escape_white_button.SetActive(false);
            }
            else if (escape_green_button.activeSelf == true)
            {
                escape_green_button.SetActive(false);
            }
        }
    }


    bool CanMoveToSquare(
        int x, int y, 
        AnimalData current_animal, 
        GridSquareBubbles[][] green_bubble_board, 
        GridSquareBubbles[][] white_bubble_board, 
        GridSquareAnimal[][] animal_board
    )
    {
        if (y < 0 || y >= 8)
        {
            return false;    
        }

        if (x < 0 || x >= 8)
        {
            return false;    
        }

        // Don't move if we are moving to the same square we are on
        if ( x == current_animal.grid.x && y == current_animal.grid.y )
        {
            return false;
        }

        if ( 
            math.abs(x - current_animal.grid.x) > 1 ||
            math.abs(y - current_animal.grid.y) > 1 
        )
        {
            return false;
        }

        GridSquareBubbles destination_square_green_bubbles = green_bubble_board[x][y];
        if (current_animal.team == Team.Green && destination_square_green_bubbles == GridSquareBubbles.BUBBLES)
        {
            return false;
        }
        GridSquareBubbles destination_square_white_bubbles = white_bubble_board[x][y];
        if (current_animal.team == Team.White && destination_square_white_bubbles == GridSquareBubbles.BUBBLES)
        {
            return false;
        }

        GridSquareAnimal destination_square_type = animal_board[x][y];

        switch ( destination_square_type )
        {
        case GridSquareAnimal.EMPTY:
            return true;
        case GridSquareAnimal.WHITE_SHARK:
            return current_animal.type == GridSquareAnimal.GREEN_SHRIMP;
        case GridSquareAnimal.WHITE_FISH:
            return current_animal.type == GridSquareAnimal.GREEN_SHARK;
        case GridSquareAnimal.WHITE_SHRIMP:
            return current_animal.type == GridSquareAnimal.GREEN_FISH;
        case GridSquareAnimal.GREEN_SHARK:
            return current_animal.type == GridSquareAnimal.WHITE_SHRIMP;
        case GridSquareAnimal.GREEN_FISH:
            return current_animal.type == GridSquareAnimal.WHITE_SHARK;
        case GridSquareAnimal.GREEN_SHRIMP:
            return current_animal.type == GridSquareAnimal.WHITE_FISH;
        }

        return true;
    }
    
    public void OnClick(int x, int y)
    {
        AnimalData current_animal = GetAnimalTurn(current_team);
        Debug.Log(x.ToString() + " " + y.ToString());
        Debug.Assert(current_animal != null);

        if ( CanMoveToSquare(
            x,y,
            current_animal, 
            green_bubble_board, 
            white_bubble_board, 
            animal_board
        ))
        {
            
            EatAnimal(current_animal.type, animal_board[x][y]);

            GridSquareAnimal animal_type = animal_board[current_animal.grid.x][current_animal.grid.y];
            Debug.Log("Set Animal: " + x.ToString() + " " + y.ToString() + " " + animal_type.ToString());
            
            SpawnBubbles(current_team, current_animal.grid.x, current_animal.grid.y);
            
            if ( current_team == Team.Green )
            {
                ChangeAnimalGreen();
            }
            else
            {
                ChangeAnimalWhite();
            }
            ChangeTeam();

            MoveAnimal( current_animal, x, y );

            current_animal.grid.x = x;
            current_animal.grid.y = y;

            current_animal = GetAnimalTurn(current_team);

            // while ( trapped && we still have animals to kill )
            //    kill animal one by one painfully

            while ( current_animal != null && Trapped(current_animal) )
            {
                KillAnimal(current_animal);
                current_animal = GetAnimalTurn(current_team);
            }

            if ( current_animal == null )
            {
                Debug.Log( current_team.ToString() + " Loses");
            }
        }
    }

    public void EscapeWhite()
    {
        if (current_team == Team.White)
        {
            if (white_animals.Count > 0)
            {
                AnimalData current_animal = GetAnimalTurn(current_team);
                if (escapable_animals.Contains(current_animal))
                {
                    if (current_animal.grid.x+1 > 7 || current_animal.grid.x-1 < 0 || current_animal.grid.y+1 > 7 || current_animal.grid.y -1 <0)
                    {
                        switch (current_animal.type)
                        {
                        case GridSquareAnimal.WHITE_SHARK:
                        current_animal.game_object.transform.position = GridToPosition(-2,3);
                        escaped_white.Add(white_shark.animal);
                        escapable_animals.Add(green_shrimp);
                        break;
                        case GridSquareAnimal.WHITE_FISH:
                        current_animal.game_object.transform.position = GridToPosition(-2,4);
                        escaped_white.Add(white_fish.animal);
                        escapable_animals.Add(green_shark);

                        break;
                        case GridSquareAnimal.WHITE_SHRIMP:
                        current_animal.game_object.transform.position = GridToPosition(-2,5);
                        escaped_white.Add(white_shrimp.animal);
                        escapable_animals.Add(green_fish);

                        break;
                        } 
                        white_animals.Remove(current_animal.animal);
                        current_animal_white = ResetCurrentAnimalIndex(current_animal_white, white_animals.Count);
                        SpawnBubbles(current_team, current_animal.grid.x, current_animal.grid.y);
                        animal_board[current_animal.grid.x][current_animal.grid.y] = GridSquareAnimal.EMPTY;
                        ChangeTeam();
                        escape_white_button.SetActive(false);
                    }
                }
                
            }
            else
            {
                Debug.Log("green wins");
            }
        }
    }

    public void EscapeGreen()
    {
        if(current_team == Team.Green)
        {
            if (green_animals.Count > 0)
            {
                AnimalData current_animal = GetAnimalTurn(current_team);
                if (escapable_animals.Contains(current_animal))
                {
                    if (current_animal.grid.x+1 > 7 || current_animal.grid.x-1 < 0 || current_animal.grid.y+1 > 7 || current_animal.grid.y -1 <0)
                    {
                        switch (current_animal.type)
                        {
                            case GridSquareAnimal.GREEN_SHARK:
                            current_animal.game_object.transform.position = GridToPosition(9,3);
                            escaped_green.Add(green_shark.animal);
                            escapable_animals.Add(white_shrimp);
                            break;
                            case GridSquareAnimal.GREEN_FISH:
                            current_animal.game_object.transform.position = GridToPosition(9,4);
                            escaped_green.Add(green_fish.animal);
                            escapable_animals.Add(white_shark);
                            break;
                            case GridSquareAnimal.GREEN_SHRIMP:
                            current_animal.game_object.transform.position = GridToPosition(9,5);
                            escaped_green.Add(green_shrimp.animal);
                            escapable_animals.Add(white_fish);
                            break;
                        }
                        green_animals.Remove(current_animal.animal);
                        current_animal_green = ResetCurrentAnimalIndex(current_animal_green, green_animals.Count);
                        SpawnBubbles(current_team, current_animal.grid.x, current_animal.grid.y);
                        animal_board[current_animal.grid.x][current_animal.grid.y] = GridSquareAnimal.EMPTY;
                        ChangeTeam();
                        escape_green_button.SetActive(false);
                    }
                }
                
            }
            else
            {
                Debug.Log("white wins");
            }
            
        }
    }

    

    void BubblesGoUp()
    {
        for (int i = 0; i < SpawnedBubbles.Count; i ++)
        {
            SpawnedBubbles[i].GetComponent<Rigidbody2D>().AddForce(transform.up*25);
        }
    }
}
