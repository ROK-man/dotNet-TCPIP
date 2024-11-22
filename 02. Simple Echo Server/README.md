# Simple Echo Server

기본적인 TCP 통신 예제입니다.

Socket 클래스로 리스닝 소켓을 만든 뒤,
IPEndPoint 객체를 생성해 해당 IPEndPoint에 소켓을 바인딩합니다

Accept로 클라이언트의 접속을 받아들이고,
Receive로 데이터를 받은 뒤,
Send로 되돌려주는 에코서버입니다.

클라이언트에선 Socket을 생성한 뒤 서버 주소에 Connect를 해 연결을 시도합니다.
exit를 입력하면 Shutdown, Close를 통해 접속을 종료합니다.

Receive와 Send가 순차적으로 작동해야하는 원시적인 에코 통신입니다.