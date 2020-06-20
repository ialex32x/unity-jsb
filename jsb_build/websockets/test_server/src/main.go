package main

import (
	"fmt"
	"net/http"
	"time"

	"github.com/gorilla/websocket"
)

func homepageHandler(w http.ResponseWriter, r *http.Request) {
	http.Error(w, "does not exist", 404)
}

func accept(conn *websocket.Conn) {
	for {
		tp, msg, err := conn.ReadMessage()
		if err != nil {
			fmt.Printf("failed to read %v", err)
			break
		}
		err = conn.WriteMessage(tp, msg)
		if err != nil {
			fmt.Printf("failed to write %v", err)
			break
		}
	}
}

func main() {
	originChecker := func(r *http.Request) bool {
		return true
	}
	upgrader := websocket.Upgrader{
		CheckOrigin:      originChecker,
		HandshakeTimeout: time.Minute,
	}
	http.HandleFunc("/websocket", func(w http.ResponseWriter, r *http.Request) {
		conn, err := upgrader.Upgrade(w, r, nil)
		if err != nil {
			fmt.Printf("failed to upgrade %v", r.RemoteAddr)
			return
		}
		fmt.Printf("open %v", r.RemoteAddr)
		go accept(conn)
	})
	http.HandleFunc("/", homepageHandler)
	fmt.Printf("listening...")
	http.ListenAndServe(":8080", nil)
}
