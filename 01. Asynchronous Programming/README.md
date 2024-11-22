# C#에서의 비동기 프로그래밍

닷넷에서는 Thread와 Tesk로 비동기를 수행
Thread는 .Net 초기버전부터 있었으나 특별한 경우가 아니면 잘 사용하지 않음
현재는 주로 .Net4.0에 추가된 Task를 사용(Thread에 비해 더 높은 추상화와 효율성을 지님)

Task task = new(Method);
task.Start();로 사용(Thread도 이와 같이 사용함)
Task task = Task.Run(method);로 바로 실행 가능

Task 완료 대기
(taskname).wait();
혹은 Task.WaitAny(Task[]), Task.WaitAll(Task[])을 사용

부모 스레드 자식 스레드 설정
Task task = Task.Factory.StartNew(method, TaskCreationOptions.AttachedToParent);

임계 영역 설정
object CS = new();로 빈 객체 생성 후 lock(CS); 를 사용
(내부 구현에선 lock 구문이 try Moniter.Enter(object); finally Moniter.Exit(object);로 구현됨
Moniter.TryEnter(object, TimeSpan.FromSeconds(n)); 타임아웃 설정



