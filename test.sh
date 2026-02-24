#!/bin/bash
set -e

BASE_PRODUCER_DOTNET="http://localhost:5001"
BASE_PRODUCER_NODE="http://localhost:3001"
BASE_CONSUMER_DOTNET="http://localhost:5002"
BASE_CONSUMER_NODE="http://localhost:3002"

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo ""
echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║       POC CloudEvents - Teste de Interoperabilidade         ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

echo -e "${YELLOW}[1/6] Verificando health dos serviços...${NC}"
for svc in "$BASE_PRODUCER_DOTNET" "$BASE_PRODUCER_NODE" "$BASE_CONSUMER_DOTNET" "$BASE_CONSUMER_NODE"; do
  STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$svc/health" 2>/dev/null || echo "000")
  if [ "$STATUS" = "200" ]; then
    echo -e "  ${GREEN}✓${NC} $svc -> OK"
  else
    echo "  ✗ $svc -> FALHOU ($STATUS)"
    exit 1
  fi
done

echo ""
echo -e "${YELLOW}[2/6] Limpando eventos anteriores...${NC}"
curl -s -X DELETE "$BASE_CONSUMER_DOTNET/api/events" > /dev/null
curl -s -X DELETE "$BASE_CONSUMER_NODE/api/events" > /dev/null
echo -e "  ${GREEN}✓${NC} Eventos limpos"

echo ""
echo -e "${YELLOW}[3/6] C# Producer -> Enviando evento order.created...${NC}"
RESULT=$(curl -s -X POST "$BASE_PRODUCER_DOTNET/api/events/order-created")
echo "  $RESULT" | python3 -m json.tool 2>/dev/null || echo "  $RESULT"

echo ""
echo -e "${YELLOW}[4/6] C# Producer -> Enviando evento order.shipped...${NC}"
RESULT=$(curl -s -X POST "$BASE_PRODUCER_DOTNET/api/events/order-shipped")
echo "  $RESULT" | python3 -m json.tool 2>/dev/null || echo "  $RESULT"

echo ""
echo -e "${YELLOW}[5/6] Node.js Producer -> Enviando evento user.registered...${NC}"
RESULT=$(curl -s -X POST "$BASE_PRODUCER_NODE/api/events/user-registered")
echo "  $RESULT" | python3 -m json.tool 2>/dev/null || echo "  $RESULT"

echo ""
echo -e "${YELLOW}[6/6] Node.js Producer -> Enviando evento user.updated...${NC}"
RESULT=$(curl -s -X POST "$BASE_PRODUCER_NODE/api/events/user-updated")
echo "  $RESULT" | python3 -m json.tool 2>/dev/null || echo "  $RESULT"

echo ""
echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                    Resultados                               ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

echo -e "${GREEN}Eventos recebidos pelo Consumer C#:${NC}"
curl -s "$BASE_CONSUMER_DOTNET/api/events" | python3 -m json.tool 2>/dev/null

echo ""
echo -e "${GREEN}Eventos recebidos pelo Consumer Node.js:${NC}"
curl -s "$BASE_CONSUMER_NODE/api/events" | python3 -m json.tool 2>/dev/null

echo ""
echo -e "${BLUE}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  ✓ POC CloudEvents - Teste concluído com sucesso!           ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════════╝${NC}"
