using UnityEngine;
using UnityEngine.UI;

namespace Nexon
{
    [RequireComponent( typeof( Image ) )]
    public class Casilla : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem particulasRomper4, particulasCambiarValor;
        [SerializeField]
        private GameObject animacionPuntos;
        [SerializeField]
        private Text textoBonus;

        public System.Action<Casilla> onClick;

        #region Fields
        private int valor = 0;
        public int Valor {
            get {
                return valor;
            }
            set {
                valor = value;
                ActualizarColor();
                ActualizarTexto();
            }
        }
        public Vector2Int posicion {
            get; set;
        }

        private readonly Color32[] colores = {
            new Color32(176, 176, 176, 255),
            new Color32(71, 131, 255, 255),
            new Color32(94, 202, 109, 255),
            new Color32(231, 99, 99, 255),
            new Color32(63, 63, 63, 255),
        };

        private Text texto;
        private bool initialized = false; // Usamos esta variable para evitar todos los efectos de partículas al settear el valor inicial
        #endregion Fields
        
        #region Operators
        // User-defined conversion from Casilla to Vector2
        public static implicit operator Vector2Int( Casilla v )
        {
            return v.posicion;
        }
        #endregion Operators

        #region Callbacks
#if UNITY_EDITOR
        private void OnValidate()
        {
            if( !Application.isPlaying )
            {
                if( transform.childCount == 0 )
                {
                    Debug.LogWarning( name+ " no tiene ningun hijo con el componente " + (typeof( Text )) + " para indicar su valor." );
                }
                else if ( transform.GetChild(0).GetComponent<Text>() == null )
                {
                    Debug.LogError( "El hijo de "+name+" no tiene ningún componente " + (typeof( Text )) + " para indicar su valor." );
                }
            }
        }
#endif
        private void Start()
        {
            ActualizarTexto();
            ActualizarColor();
        }
        #endregion Callbacks

        #region Private-Methods
        private void ActualizarTexto()
        {
            if ( texto == null )
                texto = GetComponentInChildren<Text>();
            texto.text = Valor.ToString();
            if( !initialized )
            {
                particulasCambiarValor.Stop();
                particulasCambiarValor.Play();
                initialized = true;
            }
        }
        private void ActualizarColor()
        {
            int idx = Valor;
            if( idx >= colores.Length )
                idx = colores.Length - 1;
            if( idx < 0 )
                idx = 0;
            GetComponent<Image>().color = colores[idx];
        }
        #endregion Private-Methods

        #region Public-Methods
        // Devuelve el nuevo valor de la casilla tras explotarla
        public int AjustarValor(int bonus)
        {
            if( Valor >= 4 )
            {
                particulasRomper4.Stop();
                particulasRomper4.Play();
                textoBonus.text = bonus > 0 ? "+" + bonus : "";
                animacionPuntos.SetActive( true );
            }
            Valor = Valor % 4;
            return Valor;
        }
        public void OnClick()
        {
            Valor++;
            if( onClick != null )
                onClick.Invoke(this);
        }
        #endregion Public-Methods
    }
}
