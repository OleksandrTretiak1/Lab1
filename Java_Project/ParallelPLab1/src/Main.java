import java.math.BigInteger;
import java.util.Scanner;
import java.util.ArrayList;
import java.util.List;

public class Main {
    public static void main(String[] args) {
        Scanner sc = new Scanner(System.in);

        while (true) {
            int stepVal;
            while (true) {
                System.out.println("\n=== Новий розрахунок Java ===");
                System.out.println("Введіть крок роботи потоків (0 для виходу):");
                if (sc.hasNextInt()) {
                    stepVal = sc.nextInt();
                    if (stepVal == 0) {
                        System.out.println("Завершення програми.");
                        return;
                    }
                    if (stepVal > 0) {
                        sc.nextLine();
                        break;
                    }
                } else {
                    sc.next();
                }
                System.out.println("Помилка: введіть ціле число більше 0.");
            }

            System.out.println("Введіть час роботи потоків у секундах через пробіл:");
            String line = sc.nextLine();
            if (line.trim().isEmpty()) continue;

            String[] timeParts = line.trim().split("\\s+");
            List<SequenceWorker> workers = new ArrayList<>();
            List<Thread> stoppers = new ArrayList<>();

            for (int i = 0; i < timeParts.length; i++) {
                try {
                    int seconds = Integer.parseInt(timeParts[i]);
                    SequenceWorker worker = new SequenceWorker(i + 1, stepVal, seconds);
                    workers.add(worker);

                    Thread stopper = new Thread(() -> {
                        try {
                            Thread.sleep(seconds * 1000L);
                        } catch (InterruptedException ignored) {}
                        worker.requestStop();
                    });
                    stoppers.add(stopper);

                } catch (NumberFormatException e) {
                    System.out.println("Помилка парсингу часу: " + timeParts[i]);
                }
            }

            for (int i = 0; i < workers.size(); i++) {
                workers.get(i).start();
                stoppers.get(i).start();
            }

            for (SequenceWorker w : workers) {
                try {
                    w.join();
                } catch (InterruptedException ignored) {}
            }

            System.out.println("Усі потоки завершили роботу. Починаємо новий цикл.");
        }
    }
}

class SequenceWorker extends Thread {
    private final int id;
    private final int step;
    private final int timeLimit;
    private volatile boolean canWork = true;

    public SequenceWorker(int id, int step, int timeLimit) {
        this.id = id;
        this.step = step;
        this.timeLimit = timeLimit;
    }

    public void requestStop() {
        this.canWork = false;
    }

    @Override
    public void run() {
        BigInteger totalSum = BigInteger.ZERO;
        BigInteger count = BigInteger.ZERO;

        BigInteger bigStep = BigInteger.valueOf(step);

        while (canWork) {
            totalSum = totalSum.add(bigStep);
            count = count.add(BigInteger.ONE);
        }

        System.out.printf("[Потік №%d] Сума: %s | Крок: %d | Доданків: %s | Час: %d сек.%n",
                id, totalSum.toString(), step, count.toString(), timeLimit);
    }
}