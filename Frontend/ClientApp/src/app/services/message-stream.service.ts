import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export class StreamMessage {
  type: string;
  data: any;
}

@Injectable()
export class MessageStreamService {
  private subject = new Subject<any>();

  constructor() { }

  sendMessage(type: string, message: any): void {
    console.log(type + ' : ' + String(message));
    this.subject.next({
      type: type,
      data: message
    });
  }

  clearMessage(): void {
    this.subject.next();
  }

  getMessage(): Observable<any> {
    return this.subject.asObservable();
  }
}
