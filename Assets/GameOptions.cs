using PoweredOn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PoweredOn
{    
    public class GameOptions : MonoBehaviour
    {
        [SerializeField]
        public float _tabSpacing = 0.07f;

        [SerializeField]
        public float TAB_SPACING 
        { 
            get { return _tabSpacing; } 
            set 
            { 
                _tabSpacing = value; 
                GameManager.Instance.game.ReflowLayout(); 
            } 
        }

        [SerializeField]
        public float _tabVertOffset = 0.02f;

        [SerializeField]
        public float TAB_VERT_OFFSET 
        { 
            get { return _tabVertOffset; } 
            set { 
                _tabVertOffset = value; 
                GameManager.Instance.game.ReflowLayout(); 
            } 
        }


        public float _zOffset = 0.0005f;

        public float Z_OFFSET
        {
            get { return _zOffset; }
            set {
                _zOffset = value;
                GameManager.Instance.game.ReflowLayout();
            }
        }

        public float _playfield_offset = -0.005f;
        public float PLAYFIELD_OFFSET {
            get { return _playfield_offset; }
            set
            {
                _playfield_offset = value;
                GameManager.Instance.game.ReflowLayout();
            }
        }

        private float _play_plane_x_rotation = 0.0f;
        
        public float PLAYPLANE_X_ROTATION
        {
            get { return _play_plane_x_rotation; }
            set
            {
                _play_plane_x_rotation = value;
                GameManager.Instance.game.ReflowLayout();
            }
        }

        // Start is called before the first frame update
        void Start()
        {            
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
