using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI GemsText;
    public void stopGame(int score)
    {
        //Time.timeScale = 0f;
        distanceText.text = score.ToString();
        GemsText.text = ControlCoin.coinCount.ToString();
        scoreText.text = score + ControlCoin.coinCount * 10 + "";
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void menu()
    {
        SceneManager.LoadScene(0);
    }
}
