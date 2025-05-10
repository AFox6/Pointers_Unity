using System.Collections;
using UnityEngine;

public class Connection : MonoBehaviour
{
    [Header("Connection Info")]
    // private Color connectionColor;

    public Node startNode {get; private set;}
    public Node endNode {get; private set;}
    private LineRenderer lr => GetComponent<LineRenderer>();

    //initialises connection
    public void StartConnection(Node _startNode, Color _color){
        startNode = _startNode;
        // transform.parent = startNode.transform; //sets parent
        
        lr.positionCount = 2;
        lr.SetPosition(0, startNode.transform.position);
        lr.SetPosition(1, startNode.transform.position);

        lr.startColor = _color;

        // ChangeColor(_color);
    }

    //sets end node for connection
    public void EndConnection(Node _endNode, Color _color){
        endNode = _endNode;
        lr.SetPosition(1, _endNode.transform.position);
        lr.endColor = _color;
    }

    //flip start and ends
    public void FlipEnds(Node _newStart, Node _newEnd){
        startNode = _newStart;
        endNode = _newEnd;
        
        lr.SetPosition(0, startNode.transform.position);
        lr.SetPosition(1, endNode.transform.position);
    }

    public void FollowPoint(Vector2 point){
        lr.SetPosition(1, point);
    }

    //follows cursor position
    public void FollowMouse(){
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        FollowPoint(mousePos);
    }

    //changes start and end colors for line renderer
    // public void ChangeColor(Color _color){
    //     connectionColor = _color;
    //     lr.startColor = connectionColor;
    //     lr.endColor = connectionColor;
    // }

    //tells point at idx to go to position in given time
    public void GoToPoint(int _idx, Vector2 _position, float _time){
        StartCoroutine(TravelToPointCoroutine(_idx, _position, _time));
    }

    //coroutine for travelling to a point
    IEnumerator TravelToPointCoroutine(int idx, Vector2 point, float moveTime){
        Vector3 startPosition = lr.GetPosition(idx);
        float elapsedTime = 0f;

        //move to pos
        while (elapsedTime < moveTime)
        {
            float t = elapsedTime / moveTime;
            lr.SetPosition(idx, Vector3.Lerp(startPosition, point, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // set final pos = target point
        lr.SetPosition(idx, point);
    }
}
