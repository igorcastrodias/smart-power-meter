"use client";

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { Line } from "react-chartjs-2";
import "moment/locale/pt-br";
import moment from "moment";
import axios from "axios";
import { useEffect, useState } from "react";

type Result = {
  deviceId: number;
  day: string;
  totalConsumption: number;
};

type Data = {
  labels: string[];
  datasets: number[][];
};

moment.locale("pt-br");

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

const options = {
  responsive: true,
  plugins: {
    legend: {
      position: "top" as const,
    },
    title: {
      display: true,
      text: "Consumo por dispositivo",
    },
  },
};

// Função para formatar os resultados em um formato adequado para o gráfico
const formatResultsForChart = (results: Result[]): Data => {
  const labels: string[] = [];
  const datasets: number[][] = [[], []];
  
  // Crie um mapa com o total de consumo para cada dia e dispositivo
  const consumptionMap: { [key: string]: number[] } = {};
  for (let result of results) {
    const date = moment(result.day).format("DD-MM-YYYY");
    if (!consumptionMap[date]) {
      consumptionMap[date] = [0, 0];
    }
    const index = result.deviceId === 1 ? 0 : 1;
    consumptionMap[date][index] = result.totalConsumption;
  }

  // Gere as datas dos últimos 30 dias
  const today = moment();
  for (let i = 0; i < 30; i++) {
    const date = moment(today).subtract(i, 'days').format("DD-MM-YYYY");
    labels.unshift(date); // Insira as datas em ordem reversa para obter uma ordem ascendente
    const consumption = consumptionMap[date] || [0, 0]; // Use o consumo do mapa, ou [0, 0] se não houver dados para essa data
    datasets[0].unshift(consumption[0]); // Insira os valores de consumo na frente do array para corresponder à ordem das datas
    datasets[1].unshift(consumption[1]);
  }

  return { labels, datasets };
};

const fetchData = async (): Promise<Data> => {
  const response = await axios.get<Result[]>(
    "https://smartpowermeter-dev.azurewebsites.net/EnergyMeasurement"
  );

  const sortedResults = sortResultsByDate(response.data);
  return formatResultsForChart(sortedResults);
};

export default function LineChart() {
  const [dados, setDados] = useState<Data>();

  useEffect(() => {
    fetchData().then((data) => setDados(data));
  }, []);

  return (
    <Line
      options={options}
      data={{
        labels: dados?.labels || [],
        datasets: [
          {
            label: "Dispositivo 1",
            data: dados?.datasets[0] || [],
            borderColor: "rgb(255, 99, 132)",
            backgroundColor: "rgba(255, 99, 132, 0.5)",
          },
          {
            label: "Dispositivo 2",
            data: dados?.datasets[1] || [],
            borderColor: "rgb(53, 162, 235)",
            backgroundColor: "rgba(53, 162, 235, 0.5)",
          },
        ],
      }}
    />
  );
}
