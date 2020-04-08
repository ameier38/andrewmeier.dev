import React from 'react'
import ReactDOM from 'react-dom'
import { BrowserRouter as Router } from 'react-router-dom'
import { ClientContext } from 'graphql-hooks'
import './index.css';
import App from './components/App'
import { client } from './graphql'
import * as serviceWorker from './serviceWorker'

ReactDOM.render(
    <React.StrictMode>
        <ClientContext.Provider value={client}>
            <Router>
                <App />
            </Router>
        </ClientContext.Provider>
    </React.StrictMode>,
    document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
