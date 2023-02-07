import React, { useEffect, useState } from "react";

import Table from "react-bootstrap/Table";
import "bootstrap/dist/css/bootstrap.min.css";
// Bootstrap Bundle JS
import "bootstrap/dist/js/bootstrap.bundle.min";
import {
  Button,
  ButtonGroup,
  Card,
  Dropdown,
  DropdownButton,
} from "react-bootstrap";
import "./App.css";

function App() {
  const [tradeList, setTradeList] = useState([]);
  const [activeExchange, setActiveExchange] = useState("CoinBase");
  const [activeAsset, setActiveAsset] = useState("BTC");
  const [activeQuote, setActiveQuote] = useState("USDT");
  const [exchangeList, setExchangeList] = useState([]);
  const [symbolList, setSymbolList] = useState([]);
  const [actAssetChanged,setActAssetChanged]=useState(false);
  useEffect(() => {
    getTrades();
    getExchanges();
    getSymbols();
  }, []);

  useEffect(() => {
      getSymbols();
      setActiveAsset("BTC");
      setActiveQuote("USDT");
      setActAssetChanged(actAssetChanged => !actAssetChanged);
     
  }, [activeExchange]);

  useEffect(() => {
    getTrades();
  }, [activeAsset, activeQuote,actAssetChanged]);

  async function getSymbols() {
    const url = "https://localhost:7007" + "/api/symbol-list/" + activeExchange;
    console.log(url);
    fetch(url, {
      method: "GET",
    })
      .then((response) => response.json())
      .then((sym) => {
        console.log(url);

        setSymbolList(sym);
      })
      .catch((error) => {
        console.log(error);
        alert(error);
      });
  }

  async function getExchanges() {
    const url = "https://localhost:7007" + "/api/exchange-list";
    console.log(url);
    fetch(url, {
      method: "GET",
    })
      .then((response) => response.json())
      .then((exchanges) => {
        console.log(exchanges);

        setExchangeList(exchanges);
      })
      .catch((error) => {
        console.log(error);
        alert(error);
      });
  }
  async function getTrades() {
    const url =
      "https://localhost:7007" +
      "/api/latest-trades/" +
      activeExchange +
      "/" +
      activeAsset +
      "-" +
      activeQuote;
    console.log(url);
    fetch(url, {
      method: "GET",
    })
      .then((response) => response.json())
      .then((trades) => {
        console.log(trades);

        setTradeList(trades);
      })
      .catch((error) => {
        console.log(error);
        alert("Invalid symbol&exchange combination");
      });
  }
  const DropDownExchange = () => (
    <Dropdown className="w-100">
      <Dropdown.Toggle variant="secondary" id="dropdown-basic">
        {activeExchange}
      </Dropdown.Toggle>

      <Dropdown.Menu style={{ maxHeight: "200px", overflowY: "auto" }}>
        {exchangeList.map((exchange) => (
          <Dropdown.Item
            key={exchange.id}
            onClick={() => {
              setActiveExchange(exchange.name);
            }}
          >
            {exchange.name}
          </Dropdown.Item>
        ))}
      </Dropdown.Menu>
    </Dropdown>
  );

  const DropDownSymbols = () => (
    <Dropdown className="w-100">
      <Dropdown.Toggle variant="secondary" id="dropdown-basic">
        {activeAsset + "-" + activeQuote}
      </Dropdown.Toggle>

      <Dropdown.Menu style={{ maxHeight: "200px", overflowY: "auto" }}>
        {symbolList.map((sym, index) => (
          <Dropdown.Item
            key={index}
            onClick={() => {
              setActiveQuote(sym.Quote.toUpperCase());
              setActiveAsset(sym.Base.toUpperCase());
            }}
          >
            {sym.Base.toUpperCase() + "-" + sym.Quote.toUpperCase()}
          </Dropdown.Item>
        ))}
      </Dropdown.Menu>
    </Dropdown>
  );


  const StripedRowTable = () => (
    <Table striped bordered hover responsive >
      <thead>
        <tr>
          <th scope="col">Trade Number</th>
          <th scope="col">Asset Symbol</th>
          <th scope="col"> Asset Quantity</th>
          <th scope="col"> Quote Symbol</th>
          <th scope="col"> Quote Quantity</th>
          <th scope="col"> Date</th>
        </tr>
      </thead>
      <tbody>
        {tradeList.map((trade) => (
          <tr key={trade.id}>
            <td>{trade.id + 1}</td>
            <td>{trade.assetSymbol}</td>
            <td>{trade.assetQuantity}</td>
            <td>{trade.quoteSymbol}</td>
            <td>{trade.quoteQuantity}</td>
            <td>{trade.date}</td>
          </tr>
        ))}
      </tbody>
    </Table>
  );

  return (
    <div style={{ display: "flex", justifyContent: "center"  }}>
      <Card>
        <Card.Header>
          <div style={{ display: "flex", alignItems: "center" }}>
            <div style={{ marginRight: "auto" }}>Latest Trades</div>
            <div
              style={{
                display: "flex",
                justifyContent: "flex-end",
                alignItems: "center",
              }}
            >
              <DropDownExchange />
              <div style={{ width: "10px" }} />
              <DropDownSymbols />
            </div>
          </div>
        </Card.Header>
        <Card.Body>
          <StripedRowTable />
        </Card.Body>
      </Card>
    </div>
  );
}

export default App;
