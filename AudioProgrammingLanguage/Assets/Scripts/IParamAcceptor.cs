using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IParamAcceptor {

	string[] AcceptableParams();
    void ConnectParam( string param, string var );
    void DisconnectParam( string param, string var );
}
