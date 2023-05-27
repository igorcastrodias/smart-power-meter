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

// Função para ordenar os resultados por data
const sortResultsByDate = (results: Result[]): Result[] => {
  return results.sort((a, b) =>
    moment(a.day).isBefore(moment(b.day)) ? -1 : 1
  );
};

// Função para formatar os resultados em um formato adequado para o gráfico
const formatResultsForChart = (results: Result[]): Data => {
  const labels: string[] = [];
  const datasets: number[][] = [[], []];

  for (let result of results) {
    if (!labels.includes(result.day)) {
      labels.push(moment(result.day).format("DD-MM"));
    }

    const index = result.deviceId === 1 ? 0 : 1;
    datasets[index].push(result.totalConsumption);
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
